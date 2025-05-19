using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HCMPo.Models;
using HCMPo.Services;
using HCMPo.Data;

namespace HCMPo.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceSyncService _attendanceSyncService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(
            IAttendanceSyncService attendanceSyncService,
            ApplicationDbContext context,
            ILogger<AttendanceController> logger)
        {
            _attendanceSyncService = attendanceSyncService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var attendances = await _context.Attendances
                .Include(a => a.Employee)
                .OrderByDescending(a => a.CheckInTime)
                .Take(100)
                .ToListAsync();

            return View(attendances);
        }

        [HttpPost]
        public async Task<IActionResult> SyncNow()
        {
            try
            {
                await _attendanceSyncService.SyncAttendanceDataAsync();
                TempData["Message"] = "Attendance data synchronized successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual attendance sync");
                TempData["Error"] = "Error during attendance sync: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RawData()
        {
            try
            {
                var rawData = await _attendanceSyncService.GetRawAttendanceDataAsync();
                return View(rawData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving raw attendance data");
                TempData["Error"] = "Error retrieving raw attendance data: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 