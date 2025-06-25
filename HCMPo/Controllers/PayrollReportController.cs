using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HCMPo.Data;
using OfficeOpenXml;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Rotativa.AspNetCore;
using HCMPo.Models;
using HCMPo.Services;
using HCMPo.Models.ViewModels;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace HCMPo.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class PayrollReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TaxCalculationService _taxService;
        private readonly ILogger<PayrollReportController> _logger;

        public PayrollReportController(ApplicationDbContext context, TaxCalculationService taxService, ILogger<PayrollReportController> logger)
        {
            _context = context;
            _taxService = taxService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string payPeriod = null, string employeeId = null, string organizationUnitId = null)
        {
            var payPeriodsData = await _context.Payrolls
                .GroupBy(p => new {
                    p.PayPeriodStart,
                    p.PayPeriodEnd
                })
                .Select(g => new {
                    PayPeriodStart = g.Key.PayPeriodStart,
                    PayPeriodEnd = g.Key.PayPeriodEnd,
                    PayPeriodStartEt = g.Select(x => x.PayPeriodStartEt).FirstOrDefault(),
                    PayPeriodEndEt = g.Select(x => x.PayPeriodEndEt).FirstOrDefault()
                })
                .OrderByDescending(p => p.PayPeriodStart)
                .ToListAsync();

            var payPeriods = payPeriodsData.Select(p => new {
                Value = p.PayPeriodStart.ToString("yyyy-MM-dd") + "," + p.PayPeriodEnd.ToString("yyyy-MM-dd"),
                Text = (p.PayPeriodStartEt?.Trim() ?? "") + " - " + (p.PayPeriodEndEt?.Trim() ?? "")
            }).ToList();

            ViewBag.PayPeriods = payPeriods;

            var employeesQuery = _context.Employees.Include(e => e.OrganizationUnit).AsQueryable();

            if (!string.IsNullOrEmpty(employeeId))
            {
                employeesQuery = employeesQuery.Where(e => e.Id == employeeId);
            }
            if (!string.IsNullOrEmpty(organizationUnitId))
            {
                employeesQuery = employeesQuery.Where(e => e.OrganizationUnitId == organizationUnitId);
            }

            var employees = await employeesQuery.Where(e => e.IsActive).ToListAsync();
            var reportViewModels = new List<PayrollReportViewModel>();
            var deductionTypes = await _context.DeductionTypes.Where(dt => dt.IsActive).OrderBy(dt => dt.Order).ToListAsync();
            var allowanceTypes = await _context.AllowanceTypes.Where(at => at.IsActive).ToListAsync();

            ViewBag.DeductionTypes = deductionTypes;
            ViewBag.AllowanceTypes = allowanceTypes;

            foreach (var employee in employees)
            {
                var employeeAllowances = await _context.EmployeeAllowances
                    .Where(ea => ea.EmployeeId == employee.Id)
                    .Include(ea => ea.AllowanceType)
                    .Where(ea => ea.AllowanceType.IsActive)
                    .ToListAsync();

                var totalAllowance = employeeAllowances.Sum(a => a.Amount);
                var grossSalary = employee.BasicSalary + totalAllowance;

                var incomeTax = await _taxService.CalculateIncomeTaxAsync(grossSalary);
                var pensionEmployee = _taxService.CalculatePensionDeduction(grossSalary);
                var pensionEmployer = grossSalary * 0.11m;

                var employeeDeductions = await _context.EmployeeDeductions
                    .Where(ed => ed.EmployeeId == employee.Id)
                    .Include(ed => ed.DeductionType)
                    .Where(ed => ed.DeductionType.IsActive)
                    .ToListAsync();

                var reportDeductions = new List<DeductionViewModel>();
                decimal otherDeductionsAmount = 0;
                foreach (var deductionType in deductionTypes)
                {
                    var employeeDeduction = employeeDeductions.FirstOrDefault(ed => ed.DeductionTypeId == deductionType.Id);
                    var amount = employeeDeduction?.Amount ?? 0;
                    reportDeductions.Add(new DeductionViewModel { DeductionName = deductionType.Name, Amount = amount });
                    otherDeductionsAmount += amount;
                }

                var totalDeductions = incomeTax + pensionEmployee + otherDeductionsAmount;
                var netPay = grossSalary - totalDeductions;

                reportViewModels.Add(new PayrollReportViewModel
                {
                    EmployeeId = employee.Id,
                    BadgeNumber = employee.BadgeNumber,
                    EmployeeName = employee.FullName,
                    BasicSalary = employee.BasicSalary,
                    TotalAllowance = totalAllowance,
                    GrossSalary = grossSalary,
                    PensionEmployer = pensionEmployer,
                    IncomeTax = incomeTax,
                    PensionEmployee = pensionEmployee,
                    Deductions = reportDeductions,
                    TotalDeductions = totalDeductions,
                    NetPay = netPay
                });
            }

            ViewBag.Employees = await _context.Employees.Where(e => e.IsActive).Select(e => new { e.Id, FullName = e.FirstName + " " + e.LastName }).ToListAsync();
            ViewBag.OrganizationUnits = await _context.OrganizationUnits.Select(ou => new { ou.Id, ou.Name }).ToListAsync();

            return View(reportViewModels);
        }

        public async Task<IActionResult> ExportExcel(DateTime? start = null, DateTime? end = null, string employeeId = null, string organizationUnitId = null)
        {
            var payrolls = _context.Payrolls.Include(p => p.Employee).AsQueryable();
            if (start.HasValue) payrolls = payrolls.Where(p => p.PayPeriodStart >= start);
            if (end.HasValue) payrolls = payrolls.Where(p => p.PayPeriodEnd <= end);
            if (!string.IsNullOrEmpty(employeeId)) payrolls = payrolls.Where(p => p.EmployeeId == employeeId);
            if (!string.IsNullOrEmpty(organizationUnitId)) payrolls = payrolls.Where(p => p.Employee.OrganizationUnitId == organizationUnitId);
            var list = await payrolls.ToListAsync();
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Payroll");
            ws.Cells[1, 1].Value = "Employee";
            ws.Cells[1, 2].Value = "Basic Salary";
            ws.Cells[1, 3].Value = "Net Salary";
            ws.Cells[1, 4].Value = "Total Deductions";
            ws.Cells[1, 5].Value = "Pay Period";
            int row = 2;
            foreach (var p in list)
            {
                ws.Cells[row, 1].Value = p.Employee?.FullName;
                ws.Cells[row, 2].Value = p.BasicSalary;
                ws.Cells[row, 3].Value = p.NetSalary;
                ws.Cells[row, 4].Value = p.TotalDeductions;
                ws.Cells[row, 5].Value = $"{p.PayPeriodStart:yyyy-MM-dd} - {p.PayPeriodEnd:yyyy-MM-dd}";
                row++;
            }
            var stream = new MemoryStream(package.GetAsByteArray());
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Payroll.xlsx");
        }

        public async Task<IActionResult> ExportCsv(DateTime? start = null, DateTime? end = null, string employeeId = null, string organizationUnitId = null)
        {
            var payrolls = _context.Payrolls.Include(p => p.Employee).AsQueryable();
            if (start.HasValue) payrolls = payrolls.Where(p => p.PayPeriodStart >= start);
            if (end.HasValue) payrolls = payrolls.Where(p => p.PayPeriodEnd <= end);
            if (!string.IsNullOrEmpty(employeeId)) payrolls = payrolls.Where(p => p.EmployeeId == employeeId);
            if (!string.IsNullOrEmpty(organizationUnitId)) payrolls = payrolls.Where(p => p.Employee.OrganizationUnitId == organizationUnitId);
            var list = await payrolls.ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Employee,Basic Salary,Net Salary,Total Deductions,Pay Period");
            foreach (var p in list)
            {
                sb.AppendLine($"{p.Employee?.FullName},{p.BasicSalary},{p.NetSalary},{p.TotalDeductions},{p.PayPeriodStart:yyyy-MM-dd} - {p.PayPeriodEnd:yyyy-MM-dd}");
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "Payroll.csv");
        }

        public async Task<IActionResult> ExportPdf(DateTime? start = null, DateTime? end = null, string employeeId = null, string organizationUnitId = null)
        {
            var payrolls = _context.Payrolls.Include(p => p.Employee).AsQueryable();
            if (start.HasValue) payrolls = payrolls.Where(p => p.PayPeriodStart >= start);
            if (end.HasValue) payrolls = payrolls.Where(p => p.PayPeriodEnd <= end);
            if (!string.IsNullOrEmpty(employeeId)) payrolls = payrolls.Where(p => p.EmployeeId == employeeId);
            if (!string.IsNullOrEmpty(organizationUnitId)) payrolls = payrolls.Where(p => p.Employee.OrganizationUnitId == organizationUnitId);
            var list = await payrolls.ToListAsync();
            return new ViewAsPdf("PdfReport", list) { FileName = "Payroll.pdf" };
        }
    }
} 