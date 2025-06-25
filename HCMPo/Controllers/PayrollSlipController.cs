using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HCMPo.Data;
using HCMPo.Models;
using HCMPo.ViewModels;
using OfficeOpenXml;
using System.IO;
using System.Text;
using Rotativa.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HCMPo.Controllers
{
    [Authorize]
    public class PayrollSlipController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public PayrollSlipController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }



        // GET: /payroll/summary?scope=all|individual&period=monthly|annual
        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Summary(string scope = "all", string period = "monthly", string employeeId = null, 
            string organizationUnitId = null, string payPeriod = null)
        {
            // Get pay periods with Ethiopian dates (similar to PayrollReport and Index)
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

            var payPeriods = payPeriodsData.Select(p => new SelectListItem
            {
                Value = p.PayPeriodStart.ToString("yyyy-MM-dd") + "," + p.PayPeriodEnd.ToString("yyyy-MM-dd"),
                Text = (p.PayPeriodStartEt?.Trim() ?? "") + " - " + (p.PayPeriodEndEt?.Trim() ?? ""),
                Selected = payPeriod == p.PayPeriodStart.ToString("yyyy-MM-dd") + "," + p.PayPeriodEnd.ToString("yyyy-MM-dd")
            }).ToList();

            ViewBag.PayPeriods = payPeriods;

            var viewModel = new PayrollSummaryViewModel
            {
                Scope = scope,
                Period = period,
                EmployeeId = employeeId,
                OrganizationUnitId = organizationUnitId
            };

            var payrollQuery = _context.Payrolls
                .Include(p => p.Employee)
                .ThenInclude(e => e.OrganizationUnit)
                .Include(p => p.Allowances)
                .ThenInclude(a => a.AllowanceType)
                .Include(p => p.Deductions)
                .ThenInclude(d => d.DeductionType)
                .AsQueryable();

            // Filter by pay period if specified
            if (!string.IsNullOrEmpty(payPeriod))
            {
                var parts = payPeriod.Split(',');
                if (parts.Length == 2 && DateTime.TryParse(parts[0], out var startG) && DateTime.TryParse(parts[1], out var endG))
                {
                    payrollQuery = payrollQuery.Where(p => p.PayPeriodStart == startG && p.PayPeriodEnd == endG);
                }
            }

            if (!string.IsNullOrEmpty(employeeId))
            {
                payrollQuery = payrollQuery.Where(p => p.EmployeeId == employeeId);
            }

            if (!string.IsNullOrEmpty(organizationUnitId))
            {
                payrollQuery = payrollQuery.Where(p => p.Employee.OrganizationUnitId == organizationUnitId);
            }

            var payrolls = await payrollQuery.ToListAsync();

            if (scope == "all")
            {
                if (period == "monthly")
                {
                    viewModel.TotalGrossSalary = payrolls.Sum(p => p.GrossSalary);
                    viewModel.TotalDeductions = payrolls.Sum(p => p.TotalDeductions);
                    viewModel.TotalNetSalary = payrolls.Sum(p => p.NetSalary);
                    viewModel.TotalEmployees = payrolls.Select(p => p.EmployeeId).Distinct().Count();
                }
                else
                {
                    var monthlyGroups = payrolls.GroupBy(p => new { p.PayPeriodStart.Month, p.PayPeriodStart.Year });
                    
                    viewModel.MonthlySummaries = monthlyGroups.Select(g => new MonthlySummary
                    {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                        GrossSalary = g.Sum(p => p.GrossSalary),
                        Deductions = g.Sum(p => p.TotalDeductions),
                        NetSalary = g.Sum(p => p.NetSalary),
                        EmployeeCount = g.Select(p => p.EmployeeId).Distinct().Count()
                    }).OrderBy(m => m.Month).ToList();

                    viewModel.TotalGrossSalary = viewModel.MonthlySummaries.Sum(m => m.GrossSalary);
                    viewModel.TotalDeductions = viewModel.MonthlySummaries.Sum(m => m.Deductions);
                    viewModel.TotalNetSalary = viewModel.MonthlySummaries.Sum(m => m.NetSalary);
                    viewModel.TotalEmployees = payrolls.Select(p => p.EmployeeId).Distinct().Count();
                }
            }
            else
            {
                var employeeGroups = payrolls.GroupBy(p => p.Employee);
                
                viewModel.EmployeeSummaries = employeeGroups.Select(g => new EmployeeSummary
                {
                    EmployeeId = g.Key.Id,
                    EmployeeName = $"{g.Key.FirstName} {g.Key.LastName}",
                    BadgeNumber = g.Key.BadgeNumber,
                    OrganizationUnit = g.Key.OrganizationUnit?.Name,
                    TotalGross = g.Sum(p => p.GrossSalary),
                    TotalDeductions = g.Sum(p => p.TotalDeductions),
                    TotalNet = g.Sum(p => p.NetSalary),
                    PayrollCount = g.Count()
                }).OrderBy(e => e.EmployeeName).ToList();

                viewModel.TotalGrossSalary = viewModel.EmployeeSummaries.Sum(e => e.TotalGross);
                viewModel.TotalDeductions = viewModel.EmployeeSummaries.Sum(e => e.TotalDeductions);
                viewModel.TotalNetSalary = viewModel.EmployeeSummaries.Sum(e => e.TotalNet);
                viewModel.TotalEmployees = viewModel.EmployeeSummaries.Count;
            }

            ViewBag.Employees = await _context.Employees
                .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                .Select(e => new SelectListItem 
                { 
                    Value = e.Id, 
                    Text = e.FirstName + " " + e.LastName,
                    Selected = employeeId == e.Id
                })
                .ToListAsync();

            ViewBag.OrganizationUnits = await _context.OrganizationUnits
                .Where(ou => ou.IsActive)
                .OrderBy(ou => ou.Name)
                .Select(ou => new SelectListItem 
                { 
                    Value = ou.Id, 
                    Text = ou.Name,
                    Selected = organizationUnitId == ou.Id
                })
                .ToListAsync();

            ViewBag.SelectedPayPeriod = payPeriod;
            ViewBag.SelectedEmployeeId = employeeId;
            ViewBag.SelectedOrganizationUnitId = organizationUnitId;
            ViewBag.SelectedScope = scope;
            ViewBag.SelectedPeriod = period;

            return View(viewModel);
        }

        // Export methods
        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> ExportExcel(string payPeriod = null, string employeeId = null, string organizationUnitId = null)
        {
            var payrolls = await GetFilteredPayrollsAsync(payPeriod, employeeId, organizationUnitId);

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Payroll Slips");
            
            // Headers
            ws.Cells[1, 1].Value = "Employee Name";
            ws.Cells[1, 2].Value = "Badge Number";
            ws.Cells[1, 3].Value = "Department";
            ws.Cells[1, 4].Value = "Position";
            ws.Cells[1, 5].Value = "Pay Period (ET)";
            ws.Cells[1, 6].Value = "Basic Salary";
            ws.Cells[1, 7].Value = "Allowances";
            ws.Cells[1, 8].Value = "Gross Salary";
            ws.Cells[1, 9].Value = "Income Tax";
            ws.Cells[1, 10].Value = "Pension (7%)";
            ws.Cells[1, 11].Value = "Other Deductions";
            ws.Cells[1, 12].Value = "Total Deductions";
            ws.Cells[1, 13].Value = "Net Salary";

            int row = 2;
            foreach (var p in payrolls)
            {
                var allowances = p.Allowances?.Sum(a => a.Amount) ?? 0;
                ws.Cells[row, 1].Value = $"{p.Employee.FirstName} {p.Employee.LastName}";
                ws.Cells[row, 2].Value = p.Employee.BadgeNumber;
                ws.Cells[row, 3].Value = p.Employee.OrganizationUnit?.Name ?? "";
                ws.Cells[row, 4].Value = p.Employee.Position ?? "";
                ws.Cells[row, 5].Value = $"{p.PayPeriodStartEt} - {p.PayPeriodEndEt}";
                ws.Cells[row, 6].Value = p.BasicSalary;
                ws.Cells[row, 7].Value = allowances;
                ws.Cells[row, 8].Value = p.GrossSalary;
                ws.Cells[row, 9].Value = p.IncomeTax;
                ws.Cells[row, 10].Value = p.PensionDeduction;
                ws.Cells[row, 11].Value = p.OtherDeductions;
                ws.Cells[row, 12].Value = p.TotalDeductions;
                ws.Cells[row, 13].Value = p.NetSalary;
                row++;
            }

            // Format headers
            using (var range = ws.Cells[1, 1, 1, 13])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            ws.Cells.AutoFitColumns();

            var stream = new MemoryStream(package.GetAsByteArray());
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PayrollSlips.xlsx");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> ExportCsv(string payPeriod = null, string employeeId = null, string organizationUnitId = null)
        {
            var payrolls = await GetFilteredPayrollsAsync(payPeriod, employeeId, organizationUnitId);

            var sb = new StringBuilder();
            sb.AppendLine("Employee Name,Badge Number,Department,Position,Pay Period (ET),Basic Salary,Allowances,Gross Salary,Income Tax,Pension (7%),Other Deductions,Total Deductions,Net Salary");
            
            foreach (var p in payrolls)
            {
                var allowances = p.Allowances?.Sum(a => a.Amount) ?? 0;
                sb.AppendLine($"\"{p.Employee.FirstName} {p.Employee.LastName}\",\"{p.Employee.BadgeNumber}\",\"{p.Employee.OrganizationUnit?.Name ?? ""}\",\"{p.Employee.Position ?? ""}\",\"{p.PayPeriodStartEt} - {p.PayPeriodEndEt}\",{p.BasicSalary},{allowances},{p.GrossSalary},{p.IncomeTax},{p.PensionDeduction},{p.OtherDeductions},{p.TotalDeductions},{p.NetSalary}");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "PayrollSlips.csv");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> ExportPdf(string payPeriod = null, string employeeId = null, string organizationUnitId = null)
        {
            var payrolls = await GetFilteredPayrollsAsync(payPeriod, employeeId, organizationUnitId);
            return new ViewAsPdf("ExportPdf", payrolls) { FileName = "PayrollSlips.pdf" };
        }

        [HttpGet]
        public async Task<IActionResult> GenerateIndividualSlips(string payPeriod = null, string employeeId = null, string organizationUnitId = null, string type = "bank")
        {
            var payrolls = await GetFilteredPayrollsAsync(payPeriod, employeeId, organizationUnitId, includeRoleCheck: true);
            
            if (!payrolls.Any())
            {
                TempData["ErrorMessage"] = "No payroll records found for the specified criteria.";
                return RedirectToAction(nameof(Index));
            }

            var slipViewModels = new List<PayrollSlipViewModel>();
            var slipConfig = await GetSlipConfigurationAsync();
            var activeStamp = await GetActiveStampAsync(type);

            foreach (var payroll in payrolls)
            {
                var slip = new PayrollSlipViewModel
                {
                    EmployeeId = payroll.EmployeeId,
                    EmployeeName = $"{payroll.Employee.FirstName} {payroll.Employee.LastName}",
                    BankAccountNumber = payroll.Employee.BankAccountNumber,
                    BankName = payroll.Employee.BankName,
                    BadgeNumber = payroll.Employee.BadgeNumber,
                    OrganizationUnit = payroll.Employee.OrganizationUnit?.Name,
                    Position = payroll.Employee.Position,
                    PayPeriodStart = payroll.PayPeriodStart,
                    PayPeriodEnd = payroll.PayPeriodEnd,
                    PayPeriodStartEt = payroll.PayPeriodStartEt,
                    PayPeriodEndEt = payroll.PayPeriodEndEt,
                    BasicSalary = payroll.BasicSalary,
                    Allowances = payroll.Allowances?.Sum(a => a.Amount) ?? 0,
                    GrossSalary = payroll.GrossSalary,
                    IncomeTax = payroll.IncomeTax,
                    PensionDeduction = payroll.PensionDeduction,
                    OtherDeductions = payroll.OtherDeductions,
                    TotalDeductions = payroll.TotalDeductions,
                    NetSalary = payroll.NetSalary,
                    WorkingDays = payroll.WorkingDays,
                    DaysWorked = payroll.DaysWorked,
                    AbsentDays = payroll.AbsentDays,
                    LateDays = payroll.LateDays,
                    SlipType = type,
                    SlipConfiguration = slipConfig,
                    ActiveStamp = activeStamp,
                    DetailedAllowances = payroll.Allowances?.ToList() ?? new List<PayrollAllowance>(),
                    DetailedDeductions = payroll.Deductions?.ToList() ?? new List<PayrollDeduction>()
                };
                slipViewModels.Add(slip);
            }

            if (type.ToLower() == "bank")
            {
                return View("BankSlip", slipViewModels);
            }
            else
            {
                return View("ReportSlip", slipViewModels);
            }
        }

        // GET: Slip management index page
        public async Task<IActionResult> Index(string payPeriod = null, string employeeId = null, string organizationUnitId = null, int page = 1)
        {
            const int pageSize = 30;
            
            // Get pay periods with Ethiopian dates (similar to PayrollReport controller)
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

            var payPeriods = payPeriodsData.Select(p => new SelectListItem
            {
                Value = p.PayPeriodStart.ToString("yyyy-MM-dd") + "," + p.PayPeriodEnd.ToString("yyyy-MM-dd"),
                Text = (p.PayPeriodStartEt?.Trim() ?? "") + " - " + (p.PayPeriodEndEt?.Trim() ?? ""),
                Selected = payPeriod == p.PayPeriodStart.ToString("yyyy-MM-dd") + "," + p.PayPeriodEnd.ToString("yyyy-MM-dd")
            }).ToList();

            ViewBag.PayPeriods = payPeriods;
            
            ViewBag.Employees = await _context.Employees
                .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                .Select(e => new SelectListItem 
                { 
                    Value = e.Id, 
                    Text = e.FirstName + " " + e.LastName,
                    Selected = employeeId == e.Id
                })
                .ToListAsync();

            ViewBag.OrganizationUnits = await _context.OrganizationUnits
                .Where(ou => ou.IsActive)
                .OrderBy(ou => ou.Name)
                .Select(ou => new SelectListItem 
                { 
                    Value = ou.Id, 
                    Text = ou.Name,
                    Selected = organizationUnitId == ou.Id
                })
                .ToListAsync();

            // Get filtered payroll data
            var payrollQuery = _context.Payrolls
                .Include(p => p.Employee)
                .ThenInclude(e => e.OrganizationUnit)
                .Include(p => p.Allowances)
                .ThenInclude(a => a.AllowanceType)
                .Include(p => p.Deductions)
                .ThenInclude(d => d.DeductionType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(payPeriod))
            {
                var parts = payPeriod.Split(',');
                if (parts.Length == 2 && DateTime.TryParse(parts[0], out var startG) && DateTime.TryParse(parts[1], out var endG))
                {
                    payrollQuery = payrollQuery.Where(p => p.PayPeriodStart == startG && p.PayPeriodEnd == endG);
                }
            }

            if (!string.IsNullOrEmpty(employeeId))
            {
                payrollQuery = payrollQuery.Where(p => p.EmployeeId == employeeId);
            }

            if (!string.IsNullOrEmpty(organizationUnitId))
            {
                payrollQuery = payrollQuery.Where(p => p.Employee.OrganizationUnitId == organizationUnitId);
            }

            // Role-based access control
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("HR");
            
            if (!isAdmin)
            {
                // Employees can only view their own slips
                var userEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.ApplicationUserId == currentUser.Id);
                if (userEmployee != null)
                {
                    payrollQuery = payrollQuery.Where(p => p.EmployeeId == userEmployee.Id);
                }
                else
                {
                    payrollQuery = payrollQuery.Where(p => false); // No data for users without employee records
                }
            }

            // Get total count for pagination
            var totalRecords = await payrollQuery.CountAsync();

            // Apply pagination
            var payrolls = await payrollQuery
                .OrderByDescending(p => p.PayPeriodStart)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calculate pagination data
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;

            ViewBag.SelectedPayPeriod = payPeriod;
            ViewBag.SelectedEmployeeId = employeeId;
            ViewBag.SelectedOrganizationUnitId = organizationUnitId;

            return View(payrolls);
        }

        // Stamp management
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> StampManagement()
        {
            var stamps = await _context.PayrollStamps.OrderByDescending(s => s.UploadedAt).ToListAsync();
            var uploadModel = new StampUploadViewModel();
            
            ViewBag.Stamps = stamps;
            return View(uploadModel);
        }

        // Upload stamp functionality
        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> UploadStamp(StampUploadViewModel model)
        {
            if (model.StampFile != null && model.StampFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "stamps");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.StampFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.StampFile.CopyToAsync(stream);
                }

                var stamp = new PayrollStamp
                {
                    FileName = model.StampFile.FileName,
                    FilePath = $"/uploads/stamps/{fileName}",
                    FileSize = model.StampFile.Length,
                    Width = model.Width,
                    Height = model.Height,
                    Position = model.Position,
                    ApplyOnBankSlip = model.ApplyOnBankSlip,
                    ApplyOnReportSlip = model.ApplyOnReportSlip,
                    IsActive = model.IsActive,
                    UploadedAt = DateTime.UtcNow,
                    UploadedBy = User.Identity?.Name ?? "System"
                };

                _context.PayrollStamps.Add(stamp);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Stamp uploaded successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
            }

            return RedirectToAction(nameof(StampManagement));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> ToggleStamp(int id)
        {
            var stamp = await _context.PayrollStamps.FindAsync(id);
            if (stamp != null)
            {
                stamp.IsActive = !stamp.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Stamp {(stamp.IsActive ? "activated" : "deactivated")} successfully.";
            }
            return RedirectToAction(nameof(StampManagement));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> DeleteStamp(int id)
        {
            var stamp = await _context.PayrollStamps.FindAsync(id);
            if (stamp != null)
            {
                // Delete file from disk
                var filePath = Path.Combine(_environment.WebRootPath, stamp.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.PayrollStamps.Remove(stamp);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Stamp deleted successfully.";
            }
            return RedirectToAction(nameof(StampManagement));
        }

        // Helper methods
        private async Task<List<Payroll>> GetFilteredPayrollsAsync(string payPeriod = null, string employeeId = null, string organizationUnitId = null, bool includeRoleCheck = false)
        {
            var payrollQuery = _context.Payrolls
                .Include(p => p.Employee)
                .ThenInclude(e => e.OrganizationUnit)
                .Include(p => p.Allowances)
                .ThenInclude(a => a.AllowanceType)
                .Include(p => p.Deductions)
                .ThenInclude(d => d.DeductionType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(payPeriod))
            {
                var parts = payPeriod.Split(',');
                if (parts.Length == 2 && DateTime.TryParse(parts[0], out var startG) && DateTime.TryParse(parts[1], out var endG))
                {
                    payrollQuery = payrollQuery.Where(p => p.PayPeriodStart == startG && p.PayPeriodEnd == endG);
                }
            }

            if (!string.IsNullOrEmpty(employeeId))
            {
                payrollQuery = payrollQuery.Where(p => p.EmployeeId == employeeId);
            }

            if (!string.IsNullOrEmpty(organizationUnitId))
            {
                payrollQuery = payrollQuery.Where(p => p.Employee.OrganizationUnitId == organizationUnitId);
            }

            // Role-based access control
            if (includeRoleCheck)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var isAdmin = User.IsInRole("Admin") || User.IsInRole("HR");
                
                if (!isAdmin)
                {
                    var userEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.ApplicationUserId == currentUser.Id);
                    if (userEmployee != null)
                    {
                        payrollQuery = payrollQuery.Where(p => p.EmployeeId == userEmployee.Id);
                    }
                    else
                    {
                        return new List<Payroll>(); // No data for users without employee records
                    }
                }
            }

            return await payrollQuery.OrderByDescending(p => p.PayPeriodStart).ToListAsync();
        }

        private async Task<PayrollSlipConfiguration> GetSlipConfigurationAsync()
        {
            var config = await _context.PayrollSlipConfigurations.FirstOrDefaultAsync();
            if (config == null)
            {
                config = new PayrollSlipConfiguration
                {
                    CompanyName = "HCM Portal Company",
                    CompanyAddress = "123 Business Street, City",
                    CompanyPhone = "+251 11 123 4567",
                    CompanyEmail = "hr@company.com",
                    BankSlipTitle = "Bank Payment Slip",
                    ReportSlipTitle = "Payroll Report Slip",
                    FooterText = "This is a computer-generated document."
                };
            }
            return config;
        }

        private async Task<PayrollStamp?> GetActiveStampAsync(string slipType)
        {
            var stamp = await _context.PayrollStamps
                .Where(s => s.IsActive)
                .FirstOrDefaultAsync();

            if (stamp != null)
            {
                if (slipType.ToLower() == "bank" && !stamp.ApplyOnBankSlip)
                    return null;
                if (slipType.ToLower() == "report" && !stamp.ApplyOnReportSlip)
                    return null;
            }

            return stamp;
        }

        [HttpGet]
        public async Task<IActionResult> SendToBank(string payPeriod = null, string organizationUnitId = null)
        {
            DateTime? startG = null, endG = null;
            string startEt = "-", endEt = "-";
            if (!string.IsNullOrEmpty(payPeriod))
            {
                var parts = payPeriod.Split(',');
                if (parts.Length == 2 && DateTime.TryParse(parts[0], out var s) && DateTime.TryParse(parts[1], out var e))
                {
                    startG = s;
                    endG = e;
                    // Try to get Ethiopian dates from any payroll slip for this period
                    var payroll = await _context.Payrolls.FirstOrDefaultAsync(p => p.PayPeriodStart == s && p.PayPeriodEnd == e);
                    if (payroll != null)
                    {
                        startEt = payroll.PayPeriodStartEt;
                        endEt = payroll.PayPeriodEndEt;
                    }
                }
            }
            ViewBag.PayPeriodStart = startG;
            ViewBag.PayPeriodEnd = endG;
            ViewBag.PayPeriodStartEt = startEt;
            ViewBag.PayPeriodEndEt = endEt;

            var payrolls = await GetFilteredPayrollsAsync(payPeriod, null, organizationUnitId, includeRoleCheck: true);
            var slipViewModels = new List<PayrollSlipViewModel>();
            foreach (var payroll in payrolls)
            {
                slipViewModels.Add(new PayrollSlipViewModel
                {
                    EmployeeName = $"{payroll.Employee.FirstName} {payroll.Employee.LastName}",
                    BankAccountNumber = payroll.Employee.BankAccountNumber,
                    BankName = payroll.Employee.BankName,
                    NetSalary = payroll.NetSalary
                });
            }
            return View("SendToBank", slipViewModels);
        }

        // Temporary action to check for missing payroll records for a given period
        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> CheckMissingPayrolls(string payPeriod)
        {
            if (string.IsNullOrEmpty(payPeriod))
            {
                TempData["ErrorMessage"] = "Pay period is required.";
                return RedirectToAction(nameof(Index));
            }

            var parts = payPeriod.Split(',');
            if (parts.Length != 2 || !DateTime.TryParse(parts[0], out var startG) || !DateTime.TryParse(parts[1], out var endG))
            {
                TempData["ErrorMessage"] = "Invalid pay period format.";
                return RedirectToAction(nameof(Index));
            }

            // Get all active employees
            var allEmployees = await _context.Employees
                .Where(e => e.Status == EmploymentStatus.Active)
                .ToListAsync();

            // Get all payrolls for the given period
            var payrollsForPeriod = await _context.Payrolls
                .Where(p => p.PayPeriodStart == startG && p.PayPeriodEnd == endG)
                .Select(p => p.EmployeeId)
                .ToListAsync();

            // Find employees missing payroll records
            var missingEmployees = allEmployees
                .Where(e => !payrollsForPeriod.Contains(e.Id))
                .ToList();

            ViewBag.PayPeriodStart = startG;
            ViewBag.PayPeriodEnd = endG;
            ViewBag.MissingCount = missingEmployees.Count;
            ViewBag.TotalEmployees = allEmployees.Count;

            return View(missingEmployees);
        }
    }
} 