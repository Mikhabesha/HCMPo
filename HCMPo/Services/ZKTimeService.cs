using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using HCMPo.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HCMPo.Services
{
    public class ZKTimeService : IZKTimeService
    {
        private readonly string _connectionString;
        private readonly ILogger<ZKTimeService> _logger;
        private SqlConnection? _connection;

        public ZKTimeService(IConfiguration configuration, ILogger<ZKTimeService> logger)
        {
            _connectionString = configuration.GetConnectionString("ZKTimeConnection")
                ?? throw new InvalidOperationException("ZKTimeConnection string is missing in configuration.");
            _logger = logger;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing ZKTime connection");
                return false;
            }
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                _connection = new SqlConnection(_connectionString);
                await _connection.OpenAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to ZKTime");
                return false;
            }
        }

        public async Task<bool> DisconnectAsync()
        {
            try
            {
                if (_connection != null)
                {
                    await _connection.CloseAsync();
                    await _connection.DisposeAsync();
                    _connection = null;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from ZKTime");
                return false;
            }
        }

        public async Task<bool> SyncAttendancesAsync()
        {
            try
            {
                if (_connection == null)
                {
                    await ConnectAsync();
                }

                // Implementation for syncing attendances
                // This is a placeholder - implement the actual sync logic here
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing attendances from ZKTime");
                return false;
            }
        }

        public async Task<ZKTimeAttendance> GetLastAttendanceAsync()
        {
            try
            {
                if (_connection == null)
                {
                    await ConnectAsync();
                }

                var query = @"
                    SELECT TOP 1 
                        u.UserId,
                        u.UserName,
                        c.CheckTime,
                        c.CheckType,
                        c.VerifyMode,
                        c.WorkCode,
                        c.DeviceId,
                        d.DeviceName
                    FROM CheckInOut c
                    INNER JOIN UserInfo u ON c.UserId = u.UserId
                    LEFT JOIN DeviceInfo d ON c.DeviceId = d.DeviceId
                    ORDER BY c.CheckTime DESC";

                using (var command = new SqlCommand(query, _connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ZKTimeAttendance
                            {
                                UserId = reader.GetInt32(0),
                                UserName = reader.GetString(1),
                                CheckTime = reader.GetDateTime(2),
                                CheckType = reader.GetString(3),
                                VerifyMode = reader.GetString(4),
                                WorkCode = reader.IsDBNull(5) ? null : reader.GetString(5),
                                DeviceId = reader.IsDBNull(6) ? null : reader.GetString(6),
                                DeviceName = reader.IsDBNull(7) ? null : reader.GetString(7)
                            };
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last attendance from ZKTime");
                return null;
            }
        }

        public async Task<List<ZKTimeAttendance>> GetAttendanceDataAsync(DateTime startDate, DateTime endDate)
        {
            var attendances = new List<ZKTimeAttendance>();

            try
            {
                if (_connection == null)
                {
                    await ConnectAsync();
                }

                var query = @"
                    SELECT 
                        u.UserId,
                        u.UserName,
                        c.CheckTime,
                        c.CheckType,
                        c.VerifyMode,
                        c.WorkCode,
                        c.DeviceId,
                        d.DeviceName
                    FROM CheckInOut c
                    INNER JOIN UserInfo u ON c.UserId = u.UserId
                    LEFT JOIN DeviceInfo d ON c.DeviceId = d.DeviceId
                    WHERE c.CheckTime BETWEEN @StartDate AND @EndDate
                    ORDER BY c.CheckTime";

                using (var command = new SqlCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            attendances.Add(new ZKTimeAttendance
                            {
                                UserId = reader.GetInt32(0),
                                UserName = reader.GetString(1),
                                CheckTime = reader.GetDateTime(2),
                                CheckType = reader.GetString(3),
                                VerifyMode = reader.GetString(4),
                                WorkCode = reader.IsDBNull(5) ? null : reader.GetString(5),
                                DeviceId = reader.IsDBNull(6) ? null : reader.GetString(6),
                                DeviceName = reader.IsDBNull(7) ? null : reader.GetString(7)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attendance data from ZKTime");
            }

            return attendances;
        }

        public async Task<List<ZKTimeAttendance>> GetAttendanceDataByEmployeeAsync(int employeeId, DateTime startDate, DateTime endDate)
        {
            var attendances = new List<ZKTimeAttendance>();

            try
            {
                if (_connection == null)
                {
                    await ConnectAsync();
                }

                var query = @"
                    SELECT 
                        u.UserId,
                        u.UserName,
                        c.CheckTime,
                        c.CheckType,
                        c.VerifyMode,
                        c.WorkCode,
                        c.DeviceId,
                        d.DeviceName
                    FROM CheckInOut c
                    INNER JOIN UserInfo u ON c.UserId = u.UserId
                    LEFT JOIN DeviceInfo d ON c.DeviceId = d.DeviceId
                    WHERE c.UserId = @UserId 
                    AND c.CheckTime BETWEEN @StartDate AND @EndDate
                    ORDER BY c.CheckTime";

                using (var command = new SqlCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@UserId", employeeId);
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            attendances.Add(new ZKTimeAttendance
                            {
                                UserId = reader.GetInt32(0),
                                UserName = reader.GetString(1),
                                CheckTime = reader.GetDateTime(2),
                                CheckType = reader.GetString(3),
                                VerifyMode = reader.GetString(4),
                                WorkCode = reader.IsDBNull(5) ? null : reader.GetString(5),
                                DeviceId = reader.IsDBNull(6) ? null : reader.GetString(6),
                                DeviceName = reader.IsDBNull(7) ? null : reader.GetString(7)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attendance data for employee {EmployeeId} from ZKTime", employeeId);
            }

            return attendances;
        }
    }
} 
