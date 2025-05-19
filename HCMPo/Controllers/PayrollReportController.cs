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

namespace HCMPo.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class PayrollReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PayrollReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? start = null, DateTime? end = null, string employeeId = null, string departmentId = null)
        {
            var payrolls = _context.Payrolls.Include(p => p.Employee).AsQueryable();
            if (start.HasValue) payrolls = payrolls.Where(p => p.PayPeriodStart >= start);
            if (end.HasValue) payrolls = payrolls.Where(p => p.PayPeriodEnd <= end);
            if (!string.IsNullOrEmpty(employeeId)) payrolls = payrolls.Where(p => p.EmployeeId == employeeId);
            if (!string.IsNullOrEmpty(departmentId)) payrolls = payrolls.Where(p => p.Employee.DepartmentId == departmentId);
            var list = await payrolls.ToListAsync();
            ViewBag.Employees = await _context.Employees.Select(e => new { e.Id, e.FullName }).ToListAsync();
            ViewBag.Departments = await _context.Departments.Select(d => new { d.Id, d.Name }).ToListAsync();
            ViewBag.DeductionTypes = await _context.DeductionTypes.Where(dt => dt.IsActive).OrderBy(dt => dt.Order).ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> ExportExcel(DateTime? start = null, DateTime? end = null, string employeeId = null, string departmentId = null)
        {
            var payrolls = _context.Payrolls.Include(p => p.Employee).AsQueryable();
            if (start.HasValue) payrolls = payrolls.Where(p => p.PayPeriodStart >= start);
            if (end.HasValue) payrolls = payrolls.Where(p => p.PayPeriodEnd <= end);
            if (!string.IsNullOrEmpty(employeeId)) payrolls = payrolls.Where(p => p.EmployeeId == employeeId);
            if (!string.IsNullOrEmpty(departmentId)) payrolls = payrolls.Where(p => p.Employee.DepartmentId == departmentId);
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

        public async Task<IActionResult> ExportCsv(DateTime? start = null, DateTime? end = null, string employeeId = null, string departmentId = null)
        {
            var payrolls = _context.Payrolls.Include(p => p.Employee).AsQueryable();
            if (start.HasValue) payrolls = payrolls.Where(p => p.PayPeriodStart >= start);
            if (end.HasValue) payrolls = payrolls.Where(p => p.PayPeriodEnd <= end);
            if (!string.IsNullOrEmpty(employeeId)) payrolls = payrolls.Where(p => p.EmployeeId == employeeId);
            if (!string.IsNullOrEmpty(departmentId)) payrolls = payrolls.Where(p => p.Employee.DepartmentId == departmentId);
            var list = await payrolls.ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Employee,Basic Salary,Net Salary,Total Deductions,Pay Period");
            foreach (var p in list)
            {
                sb.AppendLine($"{p.Employee?.FullName},{p.BasicSalary},{p.NetSalary},{p.TotalDeductions},{p.PayPeriodStart:yyyy-MM-dd} - {p.PayPeriodEnd:yyyy-MM-dd}");
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "Payroll.csv");
        }

        public async Task<IActionResult> ExportPdf(DateTime? start = null, DateTime? end = null, string employeeId = null, string departmentId = null)
        {
            var payrolls = _context.Payrolls.Include(p => p.Employee).AsQueryable();
            if (start.HasValue) payrolls = payrolls.Where(p => p.PayPeriodStart >= start);
            if (end.HasValue) payrolls = payrolls.Where(p => p.PayPeriodEnd <= end);
            if (!string.IsNullOrEmpty(employeeId)) payrolls = payrolls.Where(p => p.EmployeeId == employeeId);
            if (!string.IsNullOrEmpty(departmentId)) payrolls = payrolls.Where(p => p.Employee.DepartmentId == departmentId);
            var list = await payrolls.ToListAsync();
            return new ViewAsPdf("PdfReport", list) { FileName = "Payroll.pdf" };
        }
    }
} 