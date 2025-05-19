using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace HCMPo.Services
{
    public class AttendanceSyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AttendanceSyncBackgroundService> _logger;

        public AttendanceSyncBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<AttendanceSyncBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Attendance Sync Background Service is starting.");

            // Catch-up logic: run sync if last successful sync was before today at 2 AM
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<HCMPo.Data.ApplicationDbContext>();
                var syncService = scope.ServiceProvider.GetRequiredService<IAttendanceSyncService>();
                var lastSync = db.SyncLogs
                    .Where(l => l.Status == "Success")
                    .OrderByDescending(l => l.SyncTime)
                    .Select(l => l.SyncTime)
                    .FirstOrDefault();
                var today2am = DateTime.Today.AddHours(2);
                if (lastSync < today2am)
                {
                    try
                    {
                        _logger.LogInformation("Running catch-up sync on startup...");
                        await syncService.SyncEmployeesFromAttDbAsync();
                        await syncService.SyncAttendanceFromAttDbAsync();
                        _logger.LogInformation("Catch-up sync completed successfully.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred during catch-up sync.");
                    }
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = now.Date.AddDays(1).AddHours(2); // 2 AM next day
                var delay = nextRun - now;
                if (delay < TimeSpan.Zero) delay = TimeSpan.FromHours(24);

                await Task.Delay(delay, stoppingToken);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var syncService = scope.ServiceProvider.GetRequiredService<IAttendanceSyncService>();
                    try
                    {
                        await syncService.SyncEmployeesFromAttDbAsync();
                        await syncService.SyncAttendanceFromAttDbAsync();
                        _logger.LogInformation("Attendance sync completed successfully.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred during attendance sync.");
                    }
                }
            }

            _logger.LogInformation("Attendance Sync Background Service is stopping.");
        }
    }
}
