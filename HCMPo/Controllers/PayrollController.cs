using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HCMPo.Models;
using HCMPo.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using HCMPo.Data;
using HCMPo.ViewModels;
using HCMPo.Services;
using Microsoft.Extensions.Logging;

namespace HCMPo.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class PayrollController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AttendanceSummaryService _attendanceSummaryService;
        private readonly ILogger<PayrollController> _logger;
        private readonly TaxCalculationService _taxCalculationService;

        public PayrollController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, AttendanceSummaryService attendanceSummaryService, ILogger<PayrollController> logger, TaxCalculationService taxCalculationService)
        {
            _context = context;
            _userManager = userManager;
            _attendanceSummaryService = attendanceSummaryService;
            _logger = logger;
            _taxCalculationService = taxCalculationService;
        }

        // GET: Payroll
        public async Task<IActionResult> Index()
        {
            // Group payrolls by period
            var grouped = await _context.Payrolls
                .GroupBy(p => new {
                    p.PayPeriodStart,
                    p.PayPeriodEnd,
                    PayPeriodStartEt = p.PayPeriodStartEt.Trim(),
                    PayPeriodEndEt = p.PayPeriodEndEt.Trim()
                })
                .Select(g => new PayrollPeriodGroupViewModel
                {
                    PayPeriodStartEt = g.Key.PayPeriodStartEt,
                    PayPeriodEndEt = g.Key.PayPeriodEndEt,
                    ApprovedBy = g.Select(p => p.ApprovedBy).FirstOrDefault(),
                    PayrollCount = g.Count(),
                    PeriodId = g.Key.PayPeriodStart.ToString("yyyy-MM-dd") + "_" + g.Key.PayPeriodEnd.ToString("yyyy-MM-dd") + "_" + g.Key.PayPeriodStartEt + "_" + g.Key.PayPeriodEndEt,
                    SentDate = g.Max(p => p.SentDate),
                    IsOpened = g.All(p => p.IsOpened)
                })
                .ToListAsync();

            var ordered = grouped
                .OrderByDescending(g => g.SentDate)
                .ThenByDescending(g => g.PayPeriodStartEt)
                .ThenByDescending(g => g.PayPeriodEndEt)
                .ToList();
            return View(ordered);
        }

        public async Task<IActionResult> PayrollList(string periodId)
        {
            if (string.IsNullOrEmpty(periodId)) return NotFound();
            var parts = periodId.Split('_');
            if (parts.Length != 4) return NotFound();
            var startGregorian = DateTime.Parse(parts[0]);
            var endGregorian = DateTime.Parse(parts[1]);
            var startEt = parts[2];
            var endEt = parts[3];
            var payrolls = await _context.Payrolls
                .Where(p => p.PayPeriodStart == startGregorian && p.PayPeriodEnd == endGregorian && p.PayPeriodStartEt == startEt && p.PayPeriodEndEt == endEt)
                .Include(p => p.Employee)
                .ToListAsync();
            ViewBag.Period = $"{startEt} - {endEt}";
            foreach (var payroll in payrolls)
            {
                if (!payroll.IsOpened)
                {
                    payroll.IsOpened = true;
                    _context.Payrolls.Update(payroll);
                }
            }
            await _context.SaveChangesAsync();
            return View("PayrollList", payrolls);
        }

        // GET: Payroll/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payroll = await _context.Payrolls
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payroll == null)
            {
                return NotFound();
            }

            return View(payroll);
        }

        // GET: Payroll/Generate
        public async Task<IActionResult> Generate()
        {
            var viewModel = new Models.ViewModels.GeneratePayrollViewModel();

            // Provide employee list for optional multi-select
            ViewData["EmployeeList"] = await _context.Employees
                                                .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                                                .Select(e => new SelectListItem { Value = e.Id, Text = e.FullName })
                                                .ToListAsync();

            return View(viewModel);
        }

        // GET: Payroll/Create
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName");
            return View();
        }

        // POST: Payroll/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmployeeId,PayPeriodStart,PayPeriodStartEt,PayPeriodEnd,PayPeriodEndEt,BasicSalary,TransportAllowance,HousingAllowance,OtherAllowances,IncomeTax,PensionDeduction,OtherDeductions,WorkingDays,DaysWorked,AbsentDays,LateDays,AttendanceDeduction,GrossSalary,TotalDeductions,NetSalary,Status,GeneratedBy,GeneratedAt,ApprovedBy,ApprovedAt,Remarks,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy")] Payroll payroll)
        {
            if (ModelState.IsValid)
            {
                _context.Add(payroll);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", payroll.EmployeeId);
            return View(payroll);
        }

        // GET: Payroll/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payroll = await _context.Payrolls.FindAsync(id);
            if (payroll == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", payroll.EmployeeId);
            return View(payroll);
        }

        // POST: Payroll/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,EmployeeId,PayPeriodStart,PayPeriodStartEt,PayPeriodEnd,PayPeriodEndEt,BasicSalary,TransportAllowance,HousingAllowance,OtherAllowances,IncomeTax,PensionDeduction,OtherDeductions,WorkingDays,DaysWorked,AbsentDays,LateDays,AttendanceDeduction,GrossSalary,TotalDeductions,NetSalary,Status,GeneratedBy,GeneratedAt,ApprovedBy,ApprovedAt,Remarks,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy")] Payroll payroll)
        {
            if (id != payroll.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payroll);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PayrollExists(payroll.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", payroll.EmployeeId);
            return View(payroll);
        }

        // GET: Payroll/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payroll = await _context.Payrolls
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payroll == null)
            {
                return NotFound();
            }

            return View(payroll);
        }

        // POST: Payroll/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var payroll = await _context.Payrolls.FindAsync(id);
            if (payroll != null)
            {
                _context.Payrolls.Remove(payroll);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(Models.ViewModels.GeneratePayrollViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["EmployeeList"] = await _context.Employees
                                                    .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                                                    .Select(e => new SelectListItem { Value = e.Id, Text = e.FullName })
                                                    .ToListAsync();
                return View(model);
            }

            List<string> employeeIdsToProcess;

            if (model.SelectedEmployeeIds != null && model.SelectedEmployeeIds.Any())
            {
                employeeIdsToProcess = model.SelectedEmployeeIds;
            }
            else
            {
                employeeIdsToProcess = await _context.Employees
                                                .Where(e => e.Status == EmploymentStatus.Active)
                                                .Select(e => e.Id)
                                                .ToListAsync();
            }

            var config = new PayrollConfiguration();
            var currentUser = await _userManager.GetUserAsync(User);
            var payrolls = new List<Payroll>();

            var summaries = await _attendanceSummaryService.GetAttendanceSummariesAsync(
            model.StartDate,
            model.EndDate,
            employeeIdsToProcess
            );

            foreach (var employeeId in employeeIdsToProcess)
            {
                var employee = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Id == employeeId);

                if (employee == null) continue;

                var summary = summaries.FirstOrDefault(s => s.EmployeeId == employee.Id);
                if (summary == null) continue;

                // Get attendance records for the period
                var attendanceRecords = await _context.AttendanceRecords
                    .Where(a => a.EmployeeId == employeeId &&
                               a.Date >= model.StartDate &&
                               a.Date <= model.EndDate)
                    .ToListAsync();

                var workingDays = attendanceRecords.Count;
                var daysWorked = (decimal)summary.DaysPresent;
                var absentDays = attendanceRecords.Count(a => a.Status == AttendanceStatus.Absent);
                var lateDays = attendanceRecords.Count(a => a.Status == AttendanceStatus.Late);

                var monthlyGrossSalary = employee.BasicSalary;
                
                // Calculate net salary using the corrected logic
                var proratedGross = _taxCalculationService.CalculateProratedGrossSalary(monthlyGrossSalary, daysWorked, workingDays);
                var netSalary = await _taxCalculationService.CalculateNetSalaryAsync(proratedGross, employeeId);
                var needsReview = netSalary < 0;
                if (needsReview) netSalary = 0;
                
                // Calculate pension on prorated gross
                var pensionDeduction = _taxCalculationService.CalculatePensionDeduction(proratedGross);
                
                // Calculate tax on prorated gross
                _logger.LogDebug($"[TAX DEBUG] Calling CalculateIncomeTaxAsync with proratedGross={proratedGross} for {employee.FullName}");
                var incomeTax = await _taxCalculationService.CalculateIncomeTaxAsync(proratedGross);
                _logger.LogDebug($"[TAX DEBUG] Result from CalculateIncomeTaxAsync for {employee.FullName}: incomeTax={incomeTax}");
                
                var otherDeductions = await _taxCalculationService.CalculateOtherDeductionsAsync(employee.Id, proratedGross);
                _logger.LogDebug($"[TAX DEBUG] Other deductions for {employee.FullName}: {otherDeductions}");
                
                var totalDeductions = pensionDeduction + incomeTax + otherDeductions;

                _logger.LogInformation($"Payroll for {employee.FullName}: MonthlyGross={monthlyGrossSalary}, ProratedGross={proratedGross}, DaysWorked={daysWorked}, IncomeTax={incomeTax}, Pension={pensionDeduction}, OtherDeductions={otherDeductions}, TotalDeductions={totalDeductions}, NetSalary={netSalary}, NeedsReview={needsReview}");

                var payroll = new Payroll
                {
                    Id = Guid.NewGuid().ToString(),
                    EmployeeId = employeeId,
                    PayPeriodStart = model.StartDate,
                    PayPeriodStartEt = model.StartDateEt,
                    PayPeriodEnd = model.EndDate,
                    PayPeriodEndEt = model.EndDateEt,
                    BasicSalary = employee.BasicSalary,
                    WorkingDays = workingDays,
                    DaysWorked = daysWorked,
                    AbsentDays = absentDays,
                    LateDays = lateDays,
                    AttendanceDeduction = 0, // No attendance deduction as salary is already prorated
                    IncomeTax = incomeTax,
                    PensionDeduction = pensionDeduction,
                    OtherDeductions = otherDeductions,
                    GrossSalary = proratedGross, // Store prorated gross
                    TotalDeductions = totalDeductions,
                    NetSalary = netSalary,
                    Status = needsReview ? PayrollStatus.NeedsReview : PayrollStatus.Draft,
                    GeneratedBy = currentUser.UserName,
                    GeneratedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUser.UserName,
                    Remarks = needsReview ? "Net salary was negative, set to 0. Please review." : ""
                };

                payrolls.Add(payroll);
            }

            _context.Payrolls.AddRange(payrolls);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payroll generated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Payroll/Declarations
        public async Task<IActionResult> Declarations()
        {
            var declarations = await _context.PayrollDeclarations
                .OrderByDescending(d => d.StartDate)
                .ToListAsync();
            return View(declarations);
        }

        // GET: Payroll/ReviewDeclaration/{id}
        public async Task<IActionResult> ReviewDeclaration(string id)
        {
            var declaration = await _context.PayrollDeclarations.FirstOrDefaultAsync(d => d.Id == id);
            if (declaration == null)
            {
                return NotFound();
            }
            // Parse employee IDs and materialize as List<string>
            var employeeIds = (declaration.EmployeeIds ?? "")
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            List<Employee> employees = new List<Employee>();
            if (employeeIds.Count > 0)
            {
                int chunkSize = 1000;
                for (int i = 0; i < employeeIds.Count; i += chunkSize)
                {
                    var chunk = employeeIds.Skip(i).Take(chunkSize).ToList();
                    var chunkEmployees = await _context.Employees
                        .Where(e => chunk.Contains(e.Id))
                        .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                        .ToListAsync();
                    employees.AddRange(chunkEmployees);
                }
            }

            ViewBag.Declaration = declaration;
            return View(employees);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePayrollFromDeclaration(string declarationId)
        {
            var declaration = await _context.PayrollDeclarations.FirstOrDefaultAsync(d => d.Id == declarationId);
            if (declaration == null)
            {
                TempData["ErrorMessage"] = "Payroll declaration not found.";
                return RedirectToAction("Declarations");
            }
            var employeeIds = declaration.EmployeeIds?.Split(",", StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
            var employees = await _context.Employees
                .Where(e => employeeIds.Contains(e.Id))
                .ToListAsync();

            var etStart = new EthiopianCalendar(declaration.StartDate);
            var etEnd = new EthiopianCalendar(declaration.EndDate);
            var payPeriodStartEt = EthiopianCalendar.FormatEthiopianDate(etStart.Year, etStart.Month, etStart.Day);
            var payPeriodEndEt = EthiopianCalendar.FormatEthiopianDate(etEnd.Year, etEnd.Month, etEnd.Day);

            // Use the new AttendanceSummaryService
            var summaries = await _attendanceSummaryService.GetAttendanceSummariesAsync(declaration.StartDate, declaration.EndDate, employeeIds.ToList());
            var generatedPayrolls = new List<Payroll>();

            foreach (var emp in employees)
            {
                var summary = summaries.FirstOrDefault(s => s.EmployeeId == emp.Id);
                if (summary == null) continue;

                var monthlyGrossSalary = emp.BasicSalary;
                var daysWorked = (decimal)summary.DaysPresent;
                var proratedGross = _taxCalculationService.CalculateProratedGrossSalary(monthlyGrossSalary, daysWorked, summary.TotalWorkingDaysInPeriod);
                var pensionDeduction = _taxCalculationService.CalculatePensionDeduction(proratedGross);
                _logger.LogDebug($"[TAX DEBUG] Calling CalculateIncomeTaxAsync with proratedGross={proratedGross} for {emp.FullName}");
                var incomeTax = await _taxCalculationService.CalculateIncomeTaxAsync(proratedGross);
                _logger.LogDebug($"[TAX DEBUG] Result from CalculateIncomeTaxAsync for {emp.FullName}: incomeTax={incomeTax}");
                var otherDeductions = await _taxCalculationService.CalculateOtherDeductionsAsync(emp.Id, proratedGross);
                _logger.LogDebug($"[TAX DEBUG] Other deductions for {emp.FullName}: {otherDeductions}");
                var totalDeductions = pensionDeduction + incomeTax + otherDeductions;
                var netSalary = proratedGross - totalDeductions;
                var needsReview = false;
                if (netSalary < 0)
                {
                    netSalary = 0;
                    needsReview = true;
                }

                _logger.LogInformation($"Payroll for {emp.FullName}: MonthlyGross={monthlyGrossSalary}, ProratedGross={proratedGross}, DaysWorked={daysWorked}, IncomeTax={incomeTax}, Pension={pensionDeduction}, OtherDeductions={otherDeductions}, TotalDeductions={totalDeductions}, NetSalary={netSalary}, NeedsReview={needsReview}");

                // Fix duplicate check: compare both Gregorian and Ethiopian period fields
                var existingPayroll = await _context.Payrolls.FirstOrDefaultAsync(p =>
                    p.EmployeeId == emp.Id &&
                    p.PayPeriodStart.Date == declaration.StartDate.Date &&
                    p.PayPeriodEnd.Date == declaration.EndDate.Date &&
                    p.PayPeriodStartEt == payPeriodStartEt &&
                    p.PayPeriodEndEt == payPeriodEndEt);
                if (existingPayroll != null)
                {
                    existingPayroll.PayPeriodStartEt = payPeriodStartEt;
                    existingPayroll.PayPeriodEndEt = payPeriodEndEt;
                    existingPayroll.BasicSalary = emp.BasicSalary;
                    existingPayroll.TransportAllowance = 0;
                    existingPayroll.HousingAllowance = 0;
                    existingPayroll.OtherAllowances = 0;
                    existingPayroll.IncomeTax = incomeTax;
                    existingPayroll.PensionDeduction = pensionDeduction;
                    existingPayroll.OtherDeductions = otherDeductions;
                    existingPayroll.WorkingDays = summary.TotalWorkingDaysInPeriod;
                    existingPayroll.DaysWorked = (decimal)summary.DaysPresent;
                    existingPayroll.AbsentDays = (int)summary.DaysAbsent;
                    existingPayroll.LateDays = (int)summary.DaysLate;
                    existingPayroll.AttendanceDeduction = 0; // No attendance deduction as salary is already prorated
                    existingPayroll.GrossSalary = proratedGross;
                    existingPayroll.TotalDeductions = totalDeductions;
                    existingPayroll.NetSalary = netSalary;
                    existingPayroll.Status = needsReview ? PayrollStatus.NeedsReview : PayrollStatus.Generated;
                    existingPayroll.GeneratedAt = DateTime.UtcNow;
                    existingPayroll.GeneratedBy = User.Identity?.Name;
                    existingPayroll.CreatedAt = DateTime.UtcNow;
                    existingPayroll.CreatedBy = User.Identity?.Name;
                    existingPayroll.ApprovedBy = User.Identity?.Name;
                    existingPayroll.ApprovedAt = DateTime.UtcNow;
                    existingPayroll.ModifiedBy = User.Identity?.Name;
                    existingPayroll.ModifiedAt = DateTime.UtcNow;
                    existingPayroll.Remarks = needsReview ? "Net salary was negative, set to 0. Please review." : "";
                    existingPayroll.SentDate = DateTime.UtcNow;
                    existingPayroll.IsOpened = false;
                }
                else
                {
                    var payroll = new Payroll
                    {
                        Id = Guid.NewGuid().ToString(),
                        EmployeeId = emp.Id,
                        PayPeriodStart = declaration.StartDate,
                        PayPeriodStartEt = payPeriodStartEt,
                        PayPeriodEnd = declaration.EndDate,
                        PayPeriodEndEt = payPeriodEndEt,
                        BasicSalary = emp.BasicSalary,
                        TransportAllowance = 0,
                        HousingAllowance = 0,
                        OtherAllowances = 0,
                        IncomeTax = incomeTax,
                        PensionDeduction = pensionDeduction,
                        OtherDeductions = otherDeductions,
                        WorkingDays = summary.TotalWorkingDaysInPeriod,
                        DaysWorked = (decimal)summary.DaysPresent,
                        AbsentDays = (int)summary.DaysAbsent,
                        LateDays = (int)summary.DaysLate,
                        AttendanceDeduction = 0, // No attendance deduction as salary is already prorated
                        GrossSalary = proratedGross,
                        TotalDeductions = totalDeductions,
                        NetSalary = netSalary,
                        Status = needsReview ? PayrollStatus.NeedsReview : PayrollStatus.Generated,
                        GeneratedAt = DateTime.UtcNow,
                        GeneratedBy = User.Identity?.Name,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = User.Identity?.Name,
                        ApprovedBy = User.Identity?.Name,
                        ApprovedAt = DateTime.UtcNow,
                        ModifiedBy = User.Identity?.Name,
                        ModifiedAt = DateTime.UtcNow,
                        Remarks = needsReview ? "Net salary was negative, set to 0. Please review." : "",
                        SentDate = DateTime.UtcNow,
                        IsOpened = false
                    };
                    generatedPayrolls.Add(payroll);
                }
            }
            if (generatedPayrolls.Any())
                await _context.Payrolls.AddRangeAsync(generatedPayrolls);
            declaration.Status = PayrollDeclarationStatus.Approved;
            _context.PayrollDeclarations.Update(declaration);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payroll generated successfully for the selected period.";
            return RedirectToAction("Declarations");
        }

        // ONE-TIME: Fix Payroll Period Ethiopian Dates
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FixPayrollEtDates()
        {
            var payrolls = _context.Payrolls
                .Where(p => p.PayPeriodStartEt != null && p.PayPeriodStartEt.Contains("-"))
                .ToList();

            int fixedCount = 0;
            foreach (var payroll in payrolls)
            {
                if (DateTime.TryParse(payroll.PayPeriodStartEt, out var startG))
                {
                    var etStart = new EthiopianCalendar(startG);
                    payroll.PayPeriodStartEt = $"{etStart.Year} {EthiopianCalendar.GetEthiopianMonthName(etStart.Month)} {etStart.Day:D2}";
                }
                if (DateTime.TryParse(payroll.PayPeriodEndEt, out var endG))
                {
                    var etEnd = new EthiopianCalendar(endG);
                    payroll.PayPeriodEndEt = $"{etEnd.Year} {EthiopianCalendar.GetEthiopianMonthName(etEnd.Month)} {etEnd.Day:D2}";
                }
                _context.Payrolls.Update(payroll);
                fixedCount++;
            }
            await _context.SaveChangesAsync();
            return Content($"Fixed {fixedCount} payroll period(s) to Ethiopian date format.");
        }

        // ONE-TIME: Recalculate all payrolls with new attendance and deduction logic
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RecalculateAllPayrolls()
        {
            var payrolls = await _context.Payrolls.Include(p => p.Employee).ToListAsync();
            var holidays = await _context.Holidays.Select(h => h.Date.Date).ToListAsync();
            int updated = 0;
            foreach (var payroll in payrolls)
            {
                var emp = payroll.Employee;
                if (emp == null) continue;
                var start = payroll.PayPeriodStart.Date;
                var end = payroll.PayPeriodEnd.Date;
                var attendanceRecords = await _context.AttendanceRecords
                    .Where(a => a.EmployeeId == emp.Id && a.Date >= start && a.Date <= end)
                    .ToListAsync();
                var workingDays = 0;
                double payableDays = 0;
                var absentDays = 0;
                var lateDays = 0;
                var totalDays = (end - start).Days + 1;
                for (int i = 0; i < totalDays; i++)
                {
                    var day = start.AddDays(i);
                    bool isWeekend = day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday;
                    bool isHoliday = holidays.Contains(day);
                    workingDays++;
                    if (isWeekend || isHoliday)
                    {
                        payableDays += 1;
                        continue;
                    }
                    var dayRecords = attendanceRecords.Where(a => a.Date.Date == day).ToList();
                    int presentSlots = 0;
                    if (dayRecords.Any(r => r.CheckInTime.HasValue && r.CheckInTime.Value.TimeOfDay >= new TimeSpan(7, 0, 0) && r.CheckInTime.Value.TimeOfDay <= new TimeSpan(8, 30, 0))) presentSlots++;
                    if (dayRecords.Any(r => r.CheckInTime.HasValue && r.CheckInTime.Value.TimeOfDay >= new TimeSpan(12, 15, 0) && r.CheckInTime.Value.TimeOfDay <= new TimeSpan(13, 45, 0))) presentSlots++;
                    if (dayRecords.Any(r => r.CheckInTime.HasValue && r.CheckInTime.Value.TimeOfDay >= new TimeSpan(15, 45, 0) && r.CheckInTime.Value.TimeOfDay <= new TimeSpan(17, 15, 0))) presentSlots++;
                    if (presentSlots == 3)
                    {
                        payableDays += 1;
                    }
                    else if (presentSlots > 0)
                    {
                        payableDays += 0.5;
                    }
                    else
                    {
                        absentDays++;
                    }
                }
                // Use correct calculation logic:
                var monthlyGrossSalary = emp.BasicSalary;
                var daysWorked = (int)payableDays;
                var proratedGross = _taxCalculationService.CalculateProratedGrossSalary(monthlyGrossSalary, daysWorked, workingDays);
                var pensionDeduction = _taxCalculationService.CalculatePensionDeduction(proratedGross);
                _logger.LogDebug($"[TAX DEBUG] Calling CalculateIncomeTaxAsync with proratedGross={proratedGross} for {emp.FullName}");
                var incomeTax = await _taxCalculationService.CalculateIncomeTaxAsync(proratedGross);
                _logger.LogDebug($"[TAX DEBUG] Result from CalculateIncomeTaxAsync for {emp.FullName}: incomeTax={incomeTax}");
                var otherDeductions = await _taxCalculationService.CalculateOtherDeductionsAsync(emp.Id, proratedGross);
                _logger.LogDebug($"[TAX DEBUG] Other deductions for {emp.FullName}: {otherDeductions}");
                var totalDeductions = pensionDeduction + incomeTax + otherDeductions;
                var netSalary = proratedGross - totalDeductions;
                var needsReview = false;
                if (netSalary < 0)
                {
                    netSalary = 0;
                    needsReview = true;
                }
                payroll.WorkingDays = workingDays;
                payroll.DaysWorked = daysWorked;
                payroll.AbsentDays = absentDays;
                payroll.LateDays = lateDays;
                payroll.AttendanceDeduction = 0; // No extra attendance deduction
                payroll.IncomeTax = incomeTax;
                payroll.PensionDeduction = pensionDeduction;
                payroll.OtherDeductions = otherDeductions;
                payroll.GrossSalary = proratedGross;
                payroll.TotalDeductions = totalDeductions;
                payroll.NetSalary = netSalary;
                payroll.Status = needsReview ? PayrollStatus.NeedsReview : PayrollStatus.Generated;
                payroll.Remarks = needsReview ? "Net salary was negative, set to 0. Please review." : "";
                updated++;
            }
            await _context.SaveChangesAsync();
            return Content($"Recalculated {updated} payroll(s) with new attendance and deduction logic.");
        }

        public async Task<IActionResult> GeneratePayrollForEmployeeAsync(string employeeId)
        {
            try
            {
                var employee = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Id == employeeId);

                if (employee == null)
                {
                    return NotFound("Employee not found");
                }

                // Get current month and year
                var currentDate = DateTime.UtcNow;
                var startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                // Check for existing payroll in the current month
                var existingPayroll = await _context.Payrolls
                    .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && 
                                            p.PayPeriodStart <= endOfMonth && 
                                            p.PayPeriodEnd >= startOfMonth);

                if (existingPayroll != null)
                {
                    return BadRequest($"Payroll for {employee.FullName} already exists for {currentDate:MMMM yyyy}");
                }

                // Get attendance summary for the current month
                var summaries = await _attendanceSummaryService.GetAttendanceSummariesAsync(
                    startOfMonth,
                    endOfMonth,
                    new List<string> { employeeId }
                );

                var summary = summaries.FirstOrDefault();
                if (summary == null)
                {
                    return BadRequest($"No attendance data found for {employee.FullName} in {currentDate:MMMM yyyy}");
                }

                // Use stored net salary (after-tax) from employee creation
                var afterTax = employee.Salary;
                var dailyRate = afterTax / 30m;

                // Calculate attendance deductions from daily rate
                var attendanceDeduction = ((decimal)summary.DaysAbsent + (decimal)summary.DaysLate) * dailyRate;

                // Calculate pension deduction (7% of basic salary)
                var pensionDeduction = employee.BasicSalary * 0.07m;

                // Calculate total deductions
                var totalDeductions = attendanceDeduction + pensionDeduction;
                var grossSalary = employee.BasicSalary;

                // Calculate net salary (after-tax minus attendance deductions)
                var netSalary = afterTax - attendanceDeduction;

                // Debug logging for calculation
               // _logger.LogInformation($"Payroll for {employee.FullName}: Basic={grossSalary}, IncomeTax={incomeTax}, Pension={pensionDeduction}, DailyRate={dailyRate}, PresentDays={summary.DaysPresent}, AbsentDays={summary.DaysAbsent}, LateDays={summary.DaysLate}, AttendanceDeduction={attendanceDeduction}, TotalDeductions={totalDeductions}, NetSalary={netSalary}");

                // Create new payroll
                var payroll = new Payroll
                {
                    Id = Guid.NewGuid().ToString(),
                    EmployeeId = employeeId,
                    PayPeriodStart = startOfMonth,
                    PayPeriodEnd = endOfMonth,
                    BasicSalary = employee.BasicSalary,
                    GrossSalary = grossSalary,
                    NetSalary = netSalary,
                    IncomeTax = 0, // Tax is already included in the stored net salary
                    PensionDeduction = pensionDeduction,
                    WorkingDays = summary.TotalWorkingDaysInPeriod,
                    DaysWorked = (decimal)summary.DaysPresent,
                    AbsentDays = (int)summary.DaysAbsent,
                    LateDays = (int)summary.DaysLate,
                    AttendanceDeduction = attendanceDeduction,
                    TotalDeductions = totalDeductions,
                    Status = PayrollStatus.Draft
                };

                _context.Payrolls.Add(payroll);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Payroll generated successfully for {employee.FullName}", payrollId = payroll.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating payroll");
                return StatusCode(500, "An error occurred while generating the payroll");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePayroll(PayrollGenerationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["EmployeeList"] = await _context.Employees
                    .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                    .Select(e => new SelectListItem { Value = e.Id, Text = e.FullName })
                    .ToListAsync();
                ViewData["AllowanceTypes"] = await _context.AllowanceTypes
                    .Where(a => a.IsActive)
                    .OrderBy(a => a.Name)
                    .ToListAsync();
                return View(model);
            }

            // 1. Calculate attendance-deducted salary
            decimal attendanceDeductedSalary = await _attendanceSummaryService.CalculateAttendanceDeductedSalaryAsync(model.EmployeeId, model.StartDate, model.EndDate);

            // 2. Calculate total allowances
            decimal totalAllowances = model.Allowances?.Sum(a => a.Amount) ?? 0;

            // 3. Total salary
            decimal totalSalary = attendanceDeductedSalary + totalAllowances;

            // 4. Income tax
            _logger.LogDebug($"[TAX DEBUG] Calling CalculateIncomeTaxAsync with totalSalary={totalSalary} for {model.EmployeeId}");
            decimal incomeTax = await _taxCalculationService.CalculateIncomeTaxAsync(totalSalary);
            _logger.LogDebug($"[TAX DEBUG] Result from CalculateIncomeTaxAsync for {model.EmployeeId}: incomeTax={incomeTax}");

            // 5. Pension
            decimal pensionEmployee = totalSalary * 0.07m;
            decimal pensionEmployer = totalSalary * 0.11m;

            // 6. Other deductions
            decimal otherDeductions = model.Deductions?.Sum(d => d.Amount) ?? 0;

            // 7. Total deduction
            decimal totalDeduction = incomeTax + pensionEmployee + otherDeductions;

            // 8. Net pay
            decimal netPay = totalSalary - totalDeduction;

            // 9. Create payroll
            var payroll = new Payroll
            {
                Id = Guid.NewGuid().ToString(),
                EmployeeId = model.EmployeeId,
                PayPeriodStart = model.StartDate,
                PayPeriodEnd = model.EndDate,
                BasicSalary = model.BasicSalary,
                GrossSalary = totalSalary,
                NetSalary = netPay,
                IncomeTax = incomeTax,
                PensionDeduction = pensionEmployee,
                OtherDeductions = otherDeductions,
                TotalDeductions = totalDeduction,
                Status = PayrollStatus.Draft,
                GeneratedBy = User.Identity.Name,
                GeneratedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.Identity.Name
            };

            // 10. Add allowances
            if (model.Allowances != null)
            {
                payroll.Allowances = model.Allowances.Select(a => new PayrollAllowance
                {
                    AllowanceTypeId = a.AllowanceTypeId,
                    Amount = a.Amount,
                    Remarks = a.Remarks,
                    CreatedBy = User.Identity.Name
                }).ToList();
            }

            // 11. Add deductions
            if (model.Deductions != null)
            {
                payroll.Deductions = model.Deductions.Select(d => new PayrollDeduction
                {
                    DeductionTypeId = d.DeductionTypeId,
                    Amount = d.Amount,
                    Remarks = d.Remarks,
                    CreatedBy = User.Identity.Name
                }).ToList();
            }

            _context.Payrolls.Add(payroll);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payroll generated successfully.";
            return RedirectToAction(nameof(Details), new { id = payroll.Id });
        }

        private bool PayrollExists(string id)
        {
            return _context.Payrolls.Any(e => e.Id == id);
        }

        private async Task<decimal> CalculateDeductionAsync(string employeeId, DeductionType type, DateTime startDate, DateTime endDate)
        {
            // TODO: Implement real deduction logic or fetch from configuration
            // For now, return 0 for all
            await Task.CompletedTask;
            return 0m;
        }
    }
}
