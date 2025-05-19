using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HCMPo.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using HCMPo.ViewModels;
using NuGet.Packaging;
using Microsoft.Data.SqlClient;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.LGPLv2.Core;
using Microsoft.Extensions.Configuration;
using HCMPo.Services;
using System.Globalization;
using HCMPo.Data;

namespace HCMPo.Controllers
{
    [Authorize]
    public class AttendancesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AttendancesController> _logger;
        private readonly IZKTimeService _zkTimeService;
        private readonly IConfiguration _configuration;

        public AttendancesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<AttendancesController> logger, IZKTimeService zkTimeService, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _zkTimeService = zkTimeService;
            _configuration = configuration;
        }

        // GET: Attendances
        public async Task<IActionResult> Index(
            DateTime? startDate, DateTime? endDate, string? employeeId, int pageNumber = 1, int pageSize = 10,
            int? ethStartYear = null, int? ethStartMonth = null, int? ethStartDay = null,
            int? ethEndYear = null, int? ethEndMonth = null, int? ethEndDay = null)
        {
            _logger.LogInformation("Received filter params: ethStartYear={EthStartYear}, ethStartMonth={EthStartMonth}, ethStartDay={EthStartDay}, ethEndYear={EthEndYear}, ethEndMonth={EthEndMonth}, ethEndDay={EthEndDay}", ethStartYear, ethStartMonth, ethStartDay, ethEndYear, ethEndMonth, ethEndDay);
            // If Ethiopian date parts are provided, convert to Gregorian
            if (ethStartYear.HasValue && ethStartMonth.HasValue && ethStartDay.HasValue)
            {
                var etStart = new EthiopianCalendar(ethStartYear.Value, ethStartMonth.Value, ethStartDay.Value);
                startDate = etStart.ToGregorianDate();
                _logger.LogInformation("Converted Ethiopian start date {EthYear}-{EthMonth}-{EthDay} to Gregorian {StartDate}", ethStartYear, ethStartMonth, ethStartDay, startDate);
            }
            if (ethEndYear.HasValue && ethEndMonth.HasValue && ethEndDay.HasValue)
            {
                var etEnd = new EthiopianCalendar(ethEndYear.Value, ethEndMonth.Value, ethEndDay.Value);
                endDate = etEnd.ToGregorianDate();
                _logger.LogInformation("Converted Ethiopian end date {EthYear}-{EthMonth}-{EthDay} to Gregorian {EndDate}", ethEndYear, ethEndMonth, ethEndDay, endDate);
            }

            var query = _context.Attendances.Include(a => a.Employee).AsQueryable();
            List<Attendance> attendances;
            int totalCount;
            var dailySlots = new List<DailyAttendanceSlotViewModel>();

            // If filtering by date range, build a full date grid for all employees or a single employee
            if (startDate.HasValue && endDate.HasValue)
            {
                var allDates = Enumerable.Range(0, (endDate.Value.Date - startDate.Value.Date).Days + 1)
                    .Select(offset => startDate.Value.Date.AddDays(offset))
                    .ToList();

                List<Attendance> attendanceDict;
                if (!string.IsNullOrEmpty(employeeId))
                {
                    attendanceDict = await _context.Attendances
                        .Where(a => a.EmployeeId == employeeId && a.CheckInTime >= startDate.Value.Date && a.CheckInTime < endDate.Value.Date.AddDays(1))
                        .OrderBy(a => a.CheckInTime)
                        .ToListAsync();
                    _logger.LogInformation("Found {Count} attendance records for employee {EmployeeId} between {Start} and {End}", attendanceDict.Count, employeeId, startDate, endDate);
                }
                else
                {
                    attendanceDict = await _context.Attendances
                        .Where(a => a.CheckInTime >= startDate.Value.Date && a.CheckInTime < endDate.Value.Date.AddDays(1))
                        .OrderBy(a => a.CheckInTime)
                        .ToListAsync();
                    _logger.LogInformation("Found {Count} attendance records for all employees between {Start} and {End}", attendanceDict.Count, startDate, endDate);
                }

                var employeesToProcess = string.IsNullOrEmpty(employeeId)
                    ? await _context.Employees.ToListAsync()
                    : await _context.Employees.Where(e => e.Id == employeeId).ToListAsync();

                foreach (var emp in employeesToProcess)
                {
                    foreach (var date in allDates)
                    {
                        var records = attendanceDict.Where(a => a.EmployeeId == emp.Id && a.CheckInTime.Date == date).OrderBy(a => a.CheckInTime).ToList();
                        var slot = new DailyAttendanceSlotViewModel { Date = date };

                        // Morning: 07:00-08:30
                        slot.MorningCheckIns = records.Where(r => r.CheckInTime.TimeOfDay >= new TimeSpan(7, 0, 0) && r.CheckInTime.TimeOfDay <= new TimeSpan(8, 30, 0)).Select(r => r.CheckInTime).ToList();
                        // Afternoon: 12:15-13:45
                        slot.AfternoonCheckIns = records.Where(r => r.CheckInTime.TimeOfDay >= new TimeSpan(12, 15, 0) && r.CheckInTime.TimeOfDay <= new TimeSpan(13, 45, 0)).Select(r => r.CheckInTime).ToList();
                        // Evening: 15:45-17:15
                        slot.EveningCheckOuts = records.Where(r => r.CheckInTime.TimeOfDay >= new TimeSpan(15, 45, 0) && r.CheckInTime.TimeOfDay <= new TimeSpan(17, 15, 0)).Select(r => r.CheckInTime).ToList();

                        int presentSlots = 0;
                        if (slot.MorningCheckIns.Any()) presentSlots++;
                        if (slot.AfternoonCheckIns.Any()) presentSlots++;
                        if (slot.EveningCheckOuts.Any()) presentSlots++;

                        if (presentSlots == 3)
                        {
                            slot.Status = "Present";
                            slot.Remark = "";
                        }
                        else if (presentSlots > 0)
                        {
                            slot.Status = "Half-day";
                            slot.Remark = "Missing one or more entries";
                        }
                        else
                        {
                            slot.Status = "Absent";
                            slot.Remark = "No attendance record";
                        }

                        slot.EmployeeName = emp.FullName;
                        dailySlots.Add(slot);
                    }
                }

                attendances = new List<Attendance>(); 
                totalCount = dailySlots.Count;
            }
            else
            {
                // Apply filters
                if (startDate.HasValue)
                {
                    query = query.Where(a => a.CheckInTime >= startDate.Value.Date);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(a => a.CheckInTime < endDate.Value.Date.AddDays(1));
                }

                if (!string.IsNullOrEmpty(employeeId))
                {
                    query = query.Where(a => a.EmployeeId == employeeId);
                }

                // Get total count for pagination
                totalCount = await query.CountAsync();

                // Apply pagination
                attendances = await query
                    .OrderByDescending(a => a.CheckInTime)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }

            // Get employee list for filter dropdown
            var employees = await _context.Employees
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Select(e => new SelectListItem
                {
                    Value = e.Id,
                    Text = e.FullName
                })
                .ToListAsync();

            var viewModel = new AttendanceListViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                EmployeeId = employeeId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Attendances = attendances,
                Employees = employees,
                DailyAttendanceSlots = dailySlots
            };

            // Set ViewBag for dropdowns to preserve selection
            ViewBag.SelectedStartYear = ethStartYear;
            ViewBag.SelectedStartMonth = ethStartMonth;
            ViewBag.SelectedStartDay = ethStartDay;
            ViewBag.SelectedEndYear = ethEndYear;
            ViewBag.SelectedEndMonth = ethEndMonth;
            ViewBag.SelectedEndDay = ethEndDay;

            return View(viewModel);
        }

        // GET: Attendances/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendance = await _context.Attendances
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (attendance == null)
            {
                return NotFound();
            }

            return View(attendance);
        }

        // GET: Attendances/Create
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName");
            return View();
        }

        // POST: Attendances/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeId,CheckInTime,CheckOutTime,Status,StatusReason,ManualEntryReason")] Attendance attendance)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState is valid. Attempting to create attendance record.");
                try
                {
                    attendance.Id = Guid.NewGuid().ToString();
                    attendance.IsManualEntry = true;
                    attendance.CreatedAt = DateTime.UtcNow;
                    attendance.CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    _logger.LogInformation("Attendance object before Add: EmployeeId={EmpId}, CheckIn={CheckIn}, Status={Status}", 
                        attendance.EmployeeId, attendance.CheckInTime, attendance.Status);

                    _context.Attendances.Add(attendance);

                    _logger.LogInformation("Calling SaveChangesAsync...");
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("SaveChangesAsync completed successfully.");

                    TempData["SuccessMessage"] = "Attendance record created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbEx)
                {
                    // Log detailed DbUpdateException
                    _logger.LogError(dbEx, "Database error saving new attendance record. InnerException: {InnerEx}", dbEx.InnerException?.Message);
                    ModelState.AddModelError("", "A database error occurred while saving. Please check logs or contact support.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Generic error saving new attendance record.");
                    ModelState.AddModelError("", "An error occurred while saving the attendance record. Please try again.");
                }
            }
            else
            {
                // Existing ModelState error logging...
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, x.Value.Errors })
                    .ToArray();
                _logger.LogWarning("ModelState is invalid. Errors: {ModelStateErrors}", System.Text.Json.JsonSerializer.Serialize(errors));
                if (!ModelState.Values.SelectMany(v => v.Errors).Any(e => !string.IsNullOrEmpty(e.ErrorMessage)))
                 {
                     ModelState.AddModelError("", "Please correct the validation errors.");
                 }
            }

            // Repopulate dropdowns if returning to view
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", attendance.EmployeeId);
            return View(attendance);
        }

        // GET: Attendances/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", attendance.EmployeeId);
            return View(attendance);
        }

        // POST: Attendances/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,CheckInTime,CheckOutTime,Status,StatusReason,ManualEntryReason")] Attendance attendanceViewModel)
        {
            if (id != attendanceViewModel.Id)
            {
                return NotFound();
            }

            var attendanceToUpdate = await _context.Attendances.FindAsync(id);
            if (attendanceToUpdate == null)
            {
                ModelState.AddModelError("", "Unable to find the record to update.");
                ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", attendanceViewModel.EmployeeId);
                return View(attendanceViewModel);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    attendanceToUpdate.CheckInTime = attendanceViewModel.CheckInTime;
                    attendanceToUpdate.CheckOutTime = attendanceViewModel.CheckOutTime;
                    attendanceToUpdate.Status = attendanceViewModel.Status;
                    attendanceToUpdate.StatusReason = attendanceViewModel.StatusReason;
                    attendanceToUpdate.ManualEntryReason = attendanceViewModel.ManualEntryReason;

                    attendanceToUpdate.ModifiedAt = DateTime.UtcNow;
                    attendanceToUpdate.ModifiedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    attendanceToUpdate.IsManualEntry = true;

                    _context.Attendances.Update(attendanceToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Attendance record updated successfully.";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AttendanceExists(attendanceViewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "The record was modified by another user. Please refresh and try again.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred saving changes: {ex.Message}");
                }

                if (ModelState.IsValid) return RedirectToAction(nameof(Index));
            }

            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", attendanceToUpdate.EmployeeId);
            return View(attendanceToUpdate);
        }

        // GET: Attendances/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendance = await _context.Attendances
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (attendance == null)
            {
                return NotFound();
            }

            return View(attendance);
        }

        // POST: Attendances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance != null)
            {
                _context.Attendances.Remove(attendance);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Attendance record deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not find the record to delete.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Attendances/Summary
        public async Task<IActionResult> Summary()
        {
            var viewModel = new AttendanceSummaryQueryViewModel();

            // Provide employee list for optional multi-select
            ViewData["EmployeeList"] = await _context.Employees
                                                .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                                                .Select(e => new SelectListItem { Value = e.Id, Text = e.FullName })
                                                .ToListAsync();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Summary(AttendanceSummaryQueryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Attendance Summary Query ModelState invalid.");
                ViewData["EmployeeList"] = await _context.Employees
                                                    .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                                                    .Select(e => new SelectListItem { Value = e.Id, Text = e.FullName })
                                                    .ToListAsync();
                return View(model);
            }

            _logger.LogInformation("Processing Summary Request");
            
            // Convert Ethiopian dates if provided
            if (Request.Form.ContainsKey("ethStartYear"))
            {
                var ethStartYear = int.Parse(Request.Form["ethStartYear"]);
                var ethStartMonth = int.Parse(Request.Form["ethStartMonth"]);
                var ethStartDay = int.Parse(Request.Form["ethStartDay"]);
                
                var etStart = new EthiopianCalendar(ethStartYear, ethStartMonth, ethStartDay);
                model.StartDate = etStart.ToGregorianDate();
                
                _logger.LogInformation($"Converted Ethiopian start date {ethStartYear}-{ethStartMonth}-{ethStartDay} to Gregorian {model.StartDate:yyyy-MM-dd}");
            }
            
            if (Request.Form.ContainsKey("ethEndYear"))
            {
                var ethEndYear = int.Parse(Request.Form["ethEndYear"]);
                var ethEndMonth = int.Parse(Request.Form["ethEndMonth"]);
                var ethEndDay = int.Parse(Request.Form["ethEndDay"]);
                
                var etEnd = new EthiopianCalendar(ethEndYear, ethEndMonth, ethEndDay);
                model.EndDate = etEnd.ToGregorianDate();
                
                _logger.LogInformation($"Converted Ethiopian end date {ethEndYear}-{ethEndMonth}-{ethEndDay} to Gregorian {model.EndDate:yyyy-MM-dd}");
            }

            // Ensure we're using Date only, not time
            model.StartDate = model.StartDate.Date;
            model.EndDate = model.EndDate.Date;

            _logger.LogInformation($"Final date range for processing: {model.StartDate:yyyy-MM-dd} to {model.EndDate:yyyy-MM-dd}");

            // Calculate exact days between start and end date (inclusive)
            int totalDaysInPeriod = (model.EndDate - model.StartDate).Days + 1;
            _logger.LogInformation($"Total days in period (inclusive): {totalDaysInPeriod}");

            _logger.LogInformation("Generating Attendance Summary for period {StartDate} to {EndDate}", model.StartDate, model.EndDate);

            // Log the selected employee IDs
            if (model.SelectedEmployeeIds != null)
            {
                _logger.LogInformation("Selected Employee IDs: {EmployeeIds}", string.Join(", ", model.SelectedEmployeeIds));
            }
            else
            {
                _logger.LogInformation("No specific employees selected, will fetch all active employees");
            }

            List<string> employeeIdsToProcess;
            if (model.SelectedEmployeeIds != null && model.SelectedEmployeeIds.Any())
            {
                employeeIdsToProcess = model.SelectedEmployeeIds;
                _logger.LogInformation("Processing {EmployeeCount} selected employees", employeeIdsToProcess.Count);
            }
            else
            {
                employeeIdsToProcess = await _context.Employees
                                                .Where(e => e.Status == EmploymentStatus.Active)
                                                .Select(e => e.Id)
                                                .ToListAsync();
                _logger.LogInformation("Processing all {EmployeeCount} active employees", employeeIdsToProcess.Count);
            }

            if (!employeeIdsToProcess.Any())
            {
                _logger.LogWarning("No employees found to process");
                ModelState.AddModelError("", "No active employees found or selected.");
                ViewData["EmployeeList"] = await _context.Employees
                                                .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                                                .Select(e => new SelectListItem { Value = e.Id, Text = e.FullName })
                                                .ToListAsync();
                return View(model);
            }

            // --- Fetch data in Chunks (Simplified Queries) --- 
            int chunkSize = 1000; 
            var allEmployees = new List<Employee>();
            var allAttendances = new List<Attendance>();
            var processedEmployeeCount = 0;

            _logger.LogInformation("Starting to fetch data in chunks of {ChunkSize}. Total IDs: {TotalIds}", chunkSize, employeeIdsToProcess.Count);

            while (processedEmployeeCount < employeeIdsToProcess.Count)
            {
                var currentChunkIds = employeeIdsToProcess.Skip(processedEmployeeCount).Take(chunkSize).ToList();
                if (!currentChunkIds.Any()) break; 

                _logger.LogDebug("Fetching employee chunk starting at index {StartIndex} with {ChunkCount} IDs: {Ids}", 
                    processedEmployeeCount, currentChunkIds.Count, string.Join(", ", currentChunkIds));
                
                // Build a parameterized SQL query
                var parameters = new List<object>();
                var paramNames = new List<string>();
                
                for (int i = 0; i < currentChunkIds.Count; i++)
                {
                    var paramName = $"@p{i}";
                    parameters.Add(new SqlParameter(paramName, currentChunkIds[i]));
                    paramNames.Add(paramName);
                }
                
                var sql = $"SELECT * FROM Employees WHERE Id IN ({string.Join(",", paramNames)})";
                var employeeChunk = await _context.Employees
                    .FromSqlRaw(sql, parameters.ToArray())
                    .ToListAsync();
                
                _logger.LogDebug("Fetched {Count} employees in this chunk", employeeChunk.Count);
                allEmployees.AddRange(employeeChunk);
                processedEmployeeCount += currentChunkIds.Count;
            }
            _logger.LogInformation("Finished fetching {EmployeeCount} employees.", allEmployees.Count);

            // Step 2: Fetch all attendances within the date range for *any* employee 
            var startDate = model.StartDate.Date;
            var endDateExclusive = model.EndDate.Date.AddDays(1);

            _logger.LogInformation("Fetching attendances between {StartDate} and {EndDateExclusive}", startDate, endDateExclusive);
            var attendancesInRange = await _context.Attendances
                                            .Where(a => a.CheckInTime >= startDate && a.CheckInTime < endDateExclusive)
                                            .ToListAsync();
            _logger.LogInformation("Fetched {AttendanceCount} total attendance records in date range", attendancesInRange.Count);

            // Step 3: Filter attendances in memory to match the required employees
            var employeeIdSet = new HashSet<string>(employeeIdsToProcess);
            allAttendances = attendancesInRange.Where(a => employeeIdSet.Contains(a.EmployeeId)).ToList();
            _logger.LogInformation("Filtered to {AttendanceCount} records for the target employees", allAttendances.Count);

            // Now process the combined lists
            var employees = allEmployees.ToDictionary(e => e.Id); 
            var attendances = allAttendances; 

            _logger.LogInformation("Calculating summary for {EmployeeCount} employees with {AttendanceCount} records", 
                employees.Count, attendances.Count);

            var summaryResults = new List<EmployeeAttendanceSummaryViewModel>();
            var attendanceConfig = new AttendanceConfiguration();

            foreach (var empId in employeeIdsToProcess) 
            {
                if (!employees.TryGetValue(empId, out var employee))
                {
                    _logger.LogWarning("Employee ID {EmployeeId} found in process list but not in fetched employee data. Skipping.", empId);
                    continue; 
                }

                var empAttendances = attendances.Where(a => a.EmployeeId == empId).ToList();
                _logger.LogDebug("Processing employee {EmployeeName} with {AttendanceCount} records", 
                    employee.FullName, empAttendances.Count);

                // Group attendances by day
                var attendancesByDay = empAttendances
                    .Where(a => a.CheckInTime.Date >= model.StartDate.Date && a.CheckInTime.Date <= model.EndDate.Date)
                    .GroupBy(a => a.CheckInTime.Date)
                    .ToList();

                double presentDays = 0;
                int totalLateMinutes = 0;
                int totalHalfDays = 0;

                foreach (var dayGroup in attendancesByDay)
                {
                    var dayRecords = dayGroup.ToList();
                    int presentSlots = 0;
                    // Morning: 07:00-08:30
                    if (dayRecords.Any(r => r.CheckInTime.TimeOfDay >= new TimeSpan(7, 0, 0) && r.CheckInTime.TimeOfDay <= new TimeSpan(8, 30, 0))) presentSlots++;
                    // Afternoon: 12:15-13:45
                    if (dayRecords.Any(r => r.CheckInTime.TimeOfDay >= new TimeSpan(12, 15, 0) && r.CheckInTime.TimeOfDay <= new TimeSpan(13, 45, 0))) presentSlots++;
                    // Evening: 15:45-17:15
                    if (dayRecords.Any(r => r.CheckInTime.TimeOfDay >= new TimeSpan(15, 45, 0) && r.CheckInTime.TimeOfDay <= new TimeSpan(17, 15, 0))) presentSlots++;

                    if (presentSlots == 3)
                    {
                        presentDays += 1;
                    }
                    else if (presentSlots > 0)
                    {
                        presentDays += 0.5;
                        totalHalfDays++;
                    }

                    // Calculate late minutes for the day
                    var morningCheckIn = dayRecords
                        .Where(r => r.CheckInTime.TimeOfDay >= new TimeSpan(7, 0, 0) && r.CheckInTime.TimeOfDay <= new TimeSpan(8, 30, 0))
                        .OrderBy(r => r.CheckInTime)
                        .FirstOrDefault();

                    if (morningCheckIn != null && morningCheckIn.CheckInTime.TimeOfDay > new TimeSpan(8, 0, 0))
                    {
                        totalLateMinutes += (int)(morningCheckIn.CheckInTime.TimeOfDay - new TimeSpan(8, 0, 0)).TotalMinutes;
                    }
                }

                // Sum On Leave, On Field, Holiday
                int onLeave = empAttendances.Count(a => a.Status == AttendanceStatus.OnLeave);
                int onField = empAttendances.Count(a => a.Status == AttendanceStatus.OnField);
                int holiday = empAttendances.Count(a => a.Status == AttendanceStatus.Holiday);

                // Convert late minutes to absent days (8 hours = 1 day)
                int lateToAbsent = totalLateMinutes / 480;
                int lateMinutesRemainder = totalLateMinutes % 480;
                presentDays -= lateToAbsent;
                if (presentDays < 0) presentDays = 0;

                // Absent = Working Days - Present - On Leave - On Field - Holiday
                double absentDays = totalDaysInPeriod - presentDays - onLeave - onField - holiday;
                if (absentDays < 0) absentDays = 0;

                var summary = new EmployeeAttendanceSummaryViewModel
                {
                    EmployeeId = empId,
                    EmployeeName = employee.FullName,
                    TotalWorkingDaysInPeriod = totalDaysInPeriod,
                    DaysPresent = presentDays,
                    DaysAbsent = absentDays,
                    DaysLate = lateMinutesRemainder,
                    DaysEarlyDeparture = empAttendances.Count(a => a.Status == AttendanceStatus.EarlyDeparture),
                    DaysOnLeave = onLeave,
                    DaysOnField = onField,
                    DaysHoliday = holiday,
                    DaysWeekend = 0,
                    TotalWorkDuration = empAttendances.Where(a => a.WorkDuration.HasValue)
                                               .Aggregate(TimeSpan.Zero, (total, next) => total + next.WorkDuration.Value)
                };
                summaryResults.Add(summary);
            }

            _logger.LogInformation("Generated summary for {ResultCount} employees", summaryResults.Count);

            if (!summaryResults.Any())
            {
                _logger.LogWarning("No summary results generated for the selected criteria");
                ModelState.AddModelError("", "No attendance summary data found for the selected criteria.");
                ViewData["EmployeeList"] = await _context.Employees
                                                .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                                                .Select(e => new SelectListItem { Value = e.Id, Text = e.FullName })
                                                .ToListAsync();
                return View(model);
            }

            // Pass the date range and selected employee IDs to the view
            ViewData["StartDate"] = model.StartDate;
            ViewData["EndDate"] = model.EndDate;
            ViewData["SelectedEmployeeIds"] = model.SelectedEmployeeIds != null ? string.Join(",", model.SelectedEmployeeIds) : "";
            // Ethiopian date parts for display (ensure these are from the form, not just Request.Form)
            ViewData["EthStartYear"] = Request.Form["ethStartYear"].ToString() ?? model.StartDate.Year.ToString();
            ViewData["EthStartMonth"] = Request.Form["ethStartMonth"].ToString() ?? model.StartDate.Month.ToString();
            ViewData["EthStartDay"] = Request.Form["ethStartDay"].ToString() ?? model.StartDate.Day.ToString();
            ViewData["EthEndYear"] = Request.Form["ethEndYear"].ToString() ?? model.EndDate.Year.ToString();
            ViewData["EthEndMonth"] = Request.Form["ethEndMonth"].ToString() ?? model.EndDate.Month.ToString();
            ViewData["EthEndDay"] = Request.Form["ethEndDay"].ToString() ?? model.EndDate.Day.ToString();

            return View("SummaryResult", summaryResults);
        }

        [HttpPost]
        public async Task<IActionResult> ExportSummaryToPdf(DateTime startDate, DateTime endDate, [FromForm] List<string> selectedEmployeeIds)
        {
            // Ethiopian date parts for display in export
            ViewData["EthStartYear"] = Request.Form["ethStartYear"].ToString();
            ViewData["EthStartMonth"] = Request.Form["ethStartMonth"].ToString();
            ViewData["EthStartDay"] = Request.Form["ethStartDay"].ToString();
            ViewData["EthEndYear"] = Request.Form["ethEndYear"].ToString();
            ViewData["EthEndMonth"] = Request.Form["ethEndMonth"].ToString();
            ViewData["EthEndDay"] = Request.Form["ethEndDay"].ToString();

            _logger.LogInformation("Starting PDF export for period {StartDate} to {EndDate}", startDate, endDate);
            _logger.LogInformation("Selected Employee IDs: {EmployeeIds}", 
                selectedEmployeeIds != null ? string.Join(", ", selectedEmployeeIds) : "null");

            // Step 1: Fetch all active employees first
            var allEmployees = await _context.Employees
                .Where(e => e.Status == EmploymentStatus.Active)
                .ToListAsync();
            _logger.LogInformation("Fetched {Count} active employees", allEmployees.Count);

            // Step 2: Filter employees based on selection
            List<Employee> employeesToProcess;
            if (selectedEmployeeIds != null && selectedEmployeeIds.Any())
            {
                var selectedIdsSet = new HashSet<string>(selectedEmployeeIds);
                employeesToProcess = allEmployees.Where(e => selectedIdsSet.Contains(e.Id)).ToList();
            }
            else
            {
                employeesToProcess = allEmployees;
            }
            _logger.LogInformation("Processing {Count} employees", employeesToProcess.Count);

            if (!employeesToProcess.Any())
            {
                _logger.LogWarning("No employees found to process");
                return BadRequest("No employees found to process");
            }

            // Step 3: Fetch all attendances in the date range
            var startDateUtc = startDate.Date;
            var endDateUtc = endDate.Date.AddDays(1);
            var allAttendances = await _context.Attendances
                .Where(a => a.CheckInTime >= startDateUtc && a.CheckInTime < endDateUtc)
                .ToListAsync();
            _logger.LogInformation("Fetched {Count} attendance records", allAttendances.Count);

            // Step 4: Filter attendances in memory
            var employeeIdsSet = new HashSet<string>(employeesToProcess.Select(e => e.Id));
            var relevantAttendances = allAttendances.Where(a => employeeIdsSet.Contains(a.EmployeeId)).ToList();
            _logger.LogInformation("Filtered to {Count} relevant attendance records", relevantAttendances.Count);

            // Calculate summaries
            var summaryResults = new List<EmployeeAttendanceSummaryViewModel>();
            var attendanceConfig = new AttendanceConfiguration();

            int totalWorkingDays = 0;
            for (DateTime date = startDate; date.Date <= endDate.Date; date = date.AddDays(1))
            {
                if (attendanceConfig.IsWorkingDay(date.DayOfWeek))
                {
                    totalWorkingDays++;
                }
            }
            _logger.LogInformation("Total working days in period: {Days}", totalWorkingDays);

            foreach (var employee in employeesToProcess)
            {
                var empAttendances = relevantAttendances.Where(a => a.EmployeeId == employee.Id).ToList();
                _logger.LogDebug("Processing employee {EmployeeName} with {Count} attendance records", 
                    employee.FullName, empAttendances.Count);

                var summary = new EmployeeAttendanceSummaryViewModel
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employee.FullName,
                    TotalWorkingDaysInPeriod = totalWorkingDays,
                    DaysPresent = empAttendances.Count(a => a.Status == AttendanceStatus.Present),
                    DaysAbsent = empAttendances.Count(a => a.Status == AttendanceStatus.Absent),
                    DaysLate = empAttendances.Count(a => a.Status == AttendanceStatus.Late),
                    DaysEarlyDeparture = empAttendances.Count(a => a.Status == AttendanceStatus.EarlyDeparture),
                    DaysOnLeave = empAttendances.Count(a => a.Status == AttendanceStatus.OnLeave),
                    DaysOnField = empAttendances.Count(a => a.Status == AttendanceStatus.OnField),
                    DaysHoliday = empAttendances.Count(a => a.Status == AttendanceStatus.Holiday),
                    DaysWeekend = empAttendances.Count(a => a.Status == AttendanceStatus.Weekend),
                    TotalWorkDuration = empAttendances.Where(a => a.WorkDuration.HasValue)
                                               .Aggregate(TimeSpan.Zero, (total, next) => total + next.WorkDuration.Value)
                };
                summaryResults.Add(summary);
            }

            _logger.LogInformation("Generated {Count} summary results", summaryResults.Count);

            // Create PDF
            using (var memoryStream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 50, 50, 25, 25);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                // Add title
                var titleFont = new Font(BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 16);
                var title = new Paragraph("Attendance Summary Report", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                // Add period
                var periodFont = new Font(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 12);
                var period = new Paragraph($"Period: {startDate:dd/MM/yyyy} to {endDate:dd/MM/yyyy}", periodFont);
                period.SpacingAfter = 20f;
                document.Add(period);

                if (!summaryResults.Any())
                {
                    var noDataFont = new Font(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 12);
                    var noData = new Paragraph("No attendance data found for the selected period.", noDataFont);
                    noData.Alignment = Element.ALIGN_CENTER;
                    document.Add(noData);
                }
                else
                {
                    // Create table
                    PdfPTable table = new PdfPTable(11);
                    table.WidthPercentage = 100;
                    table.SpacingBefore = 20f;
                    table.SpacingAfter = 20f;

                    // Add headers
                    var headerFont = new Font(BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 10);
                    table.AddCell(new PdfPCell(new Phrase("Employee Name", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Working Days", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Present", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Absent", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Late", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Early Departure", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("On Leave", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("On Field", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Holiday", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Weekend", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Total Hours", headerFont)));

                    // Add data
                    var cellFont = new Font(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 9);
                    foreach (var summary in summaryResults)
                    {
                        table.AddCell(new PdfPCell(new Phrase(summary.EmployeeName, cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(summary.TotalWorkingDaysInPeriod.ToString(), cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(summary.DaysPresent.ToString(), cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(summary.DaysAbsent.ToString(), cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(summary.DaysLate.ToString(), cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(summary.DaysEarlyDeparture.ToString(), cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(summary.DaysOnLeave.ToString(), cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(summary.DaysOnField.ToString(), cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(summary.DaysHoliday.ToString(), cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(summary.DaysWeekend.ToString(), cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(summary.TotalWorkDuration.TotalHours.ToString("F1"), cellFont)));
                    }

                    document.Add(table);
                }

                document.Close();

                return File(memoryStream.ToArray(), "application/pdf", 
                    $"AttendanceSummary_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestZKTimeConnection()
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("ZKTimeConnection")))
                {
                    await connection.OpenAsync();
                    
                    // Simple query to test connection and get some basic data
                    var query = @"
                        SELECT TOP 5 
                            cio.USERID,
                            cio.CHECKTIME,
                            cio.CHECKTYPE,
                            u.BADGENUMBER,
                            u.NAME,
                            d.DEPTNAME
                        FROM CHECKINOUT cio
                        LEFT JOIN USERINFO u ON cio.USERID = u.USERID
                        LEFT JOIN DEPARTMENTS d ON u.DEFAULTDEPTID = d.DEPTID
                        ORDER BY cio.CHECKTIME DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        var results = new List<object>();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                results.Add(new
                                {
                                    UserId = reader.GetInt32(reader.GetOrdinal("USERID")),
                                    CheckTime = reader.GetDateTime(reader.GetOrdinal("CHECKTIME")),
                                    CheckType = reader.GetString(reader.GetOrdinal("CHECKTYPE")),
                                    BadgeNumber = reader.GetString(reader.GetOrdinal("BADGENUMBER")),
                                    Name = reader.GetString(reader.GetOrdinal("NAME")),
                                    Department = reader.GetString(reader.GetOrdinal("DEPTNAME"))
                                });
                            }
                        }

                        return Json(new
                        {
                            Success = true,
                            Message = "Successfully connected to ZKTime database",
                            Data = results
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing ZKTime connection");
                return Json(new
                {
                    Success = false,
                    Message = "Failed to connect to ZKTime database",
                    Error = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SyncEmployeesFromAttDb([FromQuery] string progressKey)
        {
            var syncService = HttpContext.RequestServices.GetService(typeof(IAttendanceSyncService)) as IAttendanceSyncService;
            if (syncService == null)
            {
                return StatusCode(500, "AttendanceSyncService not available");
            }
            int added = await syncService.SyncEmployeesFromAttDbAsync(progressKey);
            return Ok(new { success = true, message = $"{added} employees synced from Att_db." });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SyncAttendanceFromAttDb([FromQuery] string progressKey)
        {
            var syncService = HttpContext.RequestServices.GetService(typeof(IAttendanceSyncService)) as IAttendanceSyncService;
            if (syncService == null)
            {
                return StatusCode(500, new { success = false, message = "AttendanceSyncService not available" });
            }
            try
            {
                int added = await syncService.SyncAttendanceFromAttDbAsync(progressKey);
                return Ok(new { success = true, message = $"{added} attendance records synced from Att_db." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetSyncProgress([FromQuery] string progressKey)
        {
            var syncService = HttpContext.RequestServices.GetService(typeof(IAttendanceSyncService)) as IAttendanceSyncService;
            if (syncService == null)
            {
                return StatusCode(500, new { success = false, message = "AttendanceSyncService not available" });
            }
            int progress = syncService.GetSyncProgress(progressKey);
            int total = syncService.GetSyncTotal(progressKey);
            return Ok(new { progress, total });
        }

        [HttpPost]
        public async Task<IActionResult> PostSummaryToPayroll(DateTime startDate, DateTime endDate, [FromForm] List<string> selectedEmployeeIds)
        {
            _logger.LogInformation($"Received request to post summary to payroll for {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd} for employees: {string.Join(", ", selectedEmployeeIds ?? new List<string>())}");

            var existing = await _context.PayrollDeclarations
                .FirstOrDefaultAsync(pd => pd.StartDate == startDate && pd.EndDate == endDate);
            if (existing != null)
            {
                // Store employeeIds in TempData and pass only dates as query params
                TempData["ReplaceEmployeeIds"] = selectedEmployeeIds != null ? string.Join(",", selectedEmployeeIds) : string.Empty;
                return RedirectToAction("ConfirmReplacePayrollDeclaration", new {
                    startDate = startDate.ToString("o"),
                    endDate = endDate.ToString("o")
                });
            }

            var declaration = new PayrollDeclaration
            {
                StartDate = startDate,
                EndDate = endDate,
                EmployeeIds = selectedEmployeeIds != null ? string.Join(",", selectedEmployeeIds) : string.Empty,
                Status = PayrollDeclarationStatus.PendingAccountantReview,
                CreatedBy = User.Identity?.Name,
                CreatedAt = DateTime.UtcNow
            };
            _context.PayrollDeclarations.Add(declaration);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Attendance summary posted to payroll for review.";
            return RedirectToAction("Summary");
        }

        [HttpGet]
        public IActionResult ConfirmReplacePayrollDeclaration(string startDate, string endDate)
        {
            ViewBag.ReplaceStartDate = startDate;
            ViewBag.ReplaceEndDate = endDate;
            ViewBag.ReplaceEmployeeIds = TempData["ReplaceEmployeeIds"] as string;
            TempData.Keep("ReplaceEmployeeIds"); // Keep for the POST
            return View();
        }

        [HttpPost]
        [ActionName("ConfirmReplacePayrollDeclaration")]
        public async Task<IActionResult> ConfirmReplacePayrollDeclarationPost(string startDate, string endDate)
        {
            var parsedStart = DateTime.Parse(startDate);
            var parsedEnd = DateTime.Parse(endDate);
            var employeeIds = TempData["ReplaceEmployeeIds"] as string;
            var existing = await _context.PayrollDeclarations
                .FirstOrDefaultAsync(pd => pd.StartDate == parsedStart && pd.EndDate == parsedEnd);
            if (existing != null)
            {
                existing.EmployeeIds = employeeIds;
                existing.Status = PayrollDeclarationStatus.PendingAccountantReview;
                existing.CreatedBy = User.Identity?.Name;
                existing.CreatedAt = DateTime.UtcNow;
                _context.PayrollDeclarations.Update(existing);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Payroll declaration replaced with the new summary.";
            }
            return RedirectToAction("Summary");
        }

        private bool AttendanceExists(string id)
        {
            return _context.Attendances.Any(e => e.Id == id);
        }
    }
}
