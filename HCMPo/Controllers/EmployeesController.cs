using HCMPo.Data;
using HCMPo.Models;
using HCMPo.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HCMPo.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<EmployeesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Employees
        public async Task<IActionResult> Index(string searchTerm, string departmentId, string jobTitleId, EmploymentStatus? status)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.JobTitle)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(e => 
                    e.FirstName.ToLower().Contains(searchTerm) ||
                    e.LastName.ToLower().Contains(searchTerm) ||
                    e.Email.ToLower().Contains(searchTerm) ||
                    e.BadgeNumber.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(departmentId))
            {
                query = query.Where(e => e.DepartmentId == departmentId);
            }

            if (!string.IsNullOrEmpty(jobTitleId))
            {
                query = query.Where(e => e.JobTitleId == jobTitleId);
            }

            if (status.HasValue)
            {
                query = query.Where(e => e.Status == status.Value);
            }

            var viewModel = new EmployeeFilterViewModel
            {
                SearchTerm = searchTerm,
                DepartmentId = departmentId,
                JobTitleId = jobTitleId,
                Status = status,
                Employees = await query.ToListAsync(),
                Departments = new SelectList(_context.Departments, "Id", "Name"),
                JobTitles = new SelectList(_context.JobTitles, "Id", "Title"),
                Statuses = new SelectList(Enum.GetValues(typeof(EmploymentStatus))
                    .Cast<EmploymentStatus>()
                    .Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name")
            };

            return View(viewModel);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details action called with null ID.");
                return NotFound();
            }

            _logger.LogInformation("Fetching details for employee ID: {EmployeeId}", id);
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.JobTitle)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                _logger.LogWarning("Employee with ID: {EmployeeId} not found.", id);
                return NotFound();
            }

            // Log the Salary value after loading from DB
            _logger.LogInformation("Loaded employee details. Salary value: {SalaryValue}", employee.Salary);

            return View(employee);
        }

        // GET: Employees/Create
        [Authorize(Roles = "Admin,HR")]
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            ViewData["JobTitleId"] = new SelectList(_context.JobTitles, "Id", "Title");
            ViewBag.IncomeTaxBrackets = _context.TaxSettings.Where(t => t.Type == TaxType.IncomeTax && t.IsActive).OrderBy(t => t.MinSalary).ToList();
            ViewBag.Pension = _context.TaxSettings.FirstOrDefault(t => t.Type == TaxType.Pension && t.IsActive);
            ViewBag.OtherTaxes = _context.TaxSettings.Where(t => t.Type == TaxType.Other && t.IsActive).ToList();
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,PhoneNumber,HireDate,EmergencyContact,EmergencyPhone,DateOfBirth,EmploymentDate,BasicSalary,Salary,DepartmentId,JobTitleId,Status,Address,BadgeNumber")] Employee employee)
        {
            _logger.LogInformation("Create POST called");
            _logger.LogInformation("ModelState.IsValid: {IsValid}", ModelState.IsValid);
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        _logger.LogWarning("ModelState error for {Key}: {Error}", key, error.ErrorMessage);
                    }
                }
            }

            _logger.LogInformation("Received Salary value during Create POST: {SalaryValue}", employee.Salary);

            // Read selected taxes from form
            var selectedTaxes = Request.Form["taxes"].ToList();
            if (!selectedTaxes.Any())
            {
                ModelState.AddModelError("", "At least one tax must be selected.");
                ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
                ViewData["JobTitleId"] = new SelectList(_context.JobTitles, "Id", "Title", employee.JobTitleId);
                ViewBag.IncomeTaxBrackets = _context.TaxSettings.Where(t => t.Type == TaxType.IncomeTax && t.IsActive).OrderBy(t => t.MinSalary).ToList();
                ViewBag.Pension = _context.TaxSettings.FirstOrDefault(t => t.Type == TaxType.Pension && t.IsActive);
                ViewBag.OtherTaxes = _context.TaxSettings.Where(t => t.Type == TaxType.Other && t.IsActive).ToList();
                return View(employee);
            }

            var taxPercentages = new Dictionary<string, decimal>();
            foreach (var tax in selectedTaxes)
            {
                var percentStr = Request.Form[$"taxPercent_{tax.Replace(" ", "")}"];
                if (decimal.TryParse(percentStr, out var percent))
                    taxPercentages[tax] = percent;
            }
            // Handle 'Other' tax name
            string otherTaxName = Request.Form["otherTaxDropdown"];
            if (selectedTaxes.Contains("Other") && !string.IsNullOrEmpty(otherTaxName))
            {
                var percentStr = Request.Form["taxPercent_Other"];
                if (decimal.TryParse(percentStr, out var percent))
                    taxPercentages[otherTaxName] = percent;
                taxPercentages.Remove("Other");
            }
            // Calculate net salary
            decimal gross = employee.BasicSalary;
            decimal totalTax = 0;
            foreach (var kvp in taxPercentages)
            {
                totalTax += (gross * kvp.Value / 100m);
            }
            decimal net = gross - totalTax;
            employee.Salary = net;

            if (ModelState.IsValid)
            {
                try
                {
                    employee.Id = Guid.NewGuid().ToString();
                    _context.Employees.Add(employee);
                    _logger.LogInformation("About to call SaveChangesAsync (employee)");
                    await _context.SaveChangesAsync();
                    // Save EmployeeTax records
                    foreach (var kvp in taxPercentages)
                    {
                        var empTax = new EmployeeTax 
                        { 
                            EmployeeId = employee.Id, 
                            TaxName = kvp.Key, 
                            Percentage = kvp.Value,
                            IsActive = true
                        };
                        _context.EmployeeTaxes.Add(empTax);
                    }
                    _logger.LogInformation("About to call SaveChangesAsync (taxes)");
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("SaveChangesAsync called, employee and taxes should be saved");
                    _logger.LogInformation("Successfully created employee with ID: {EmployeeId}", employee.Id);
                    TempData["SuccessMessage"] = "Employee created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving new employee.");
                    ModelState.AddModelError("", "An error occurred while saving the employee. Please try again.");
                }
            }
            else
            {
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

            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
            ViewData["JobTitleId"] = new SelectList(_context.JobTitles, "Id", "Title", employee.JobTitleId);
            ViewBag.IncomeTaxBrackets = _context.TaxSettings.Where(t => t.Type == TaxType.IncomeTax && t.IsActive).OrderBy(t => t.MinSalary).ToList();
            ViewBag.Pension = _context.TaxSettings.FirstOrDefault(t => t.Type == TaxType.Pension && t.IsActive);
            ViewBag.OtherTaxes = _context.TaxSettings.Where(t => t.Type == TaxType.Other && t.IsActive).ToList();
            return View(employee);
        }

        // GET: Employees/Edit/5
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.EmployeeTaxes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            // Defensive: ensure no nulls in lists
            var departments = (await _context.Departments.ToListAsync() ?? new List<Department>()).Where(d => d != null && d.Id != null && d.Name != null).ToList();
            var jobTitles = (await _context.JobTitles.ToListAsync() ?? new List<JobTitle>()).Where(j => j != null && j.Id != null && j.Title != null).ToList();
            var supervisors = (await _context.Employees.Where(e => e.Id != employee.Id).ToListAsync() ?? new List<Employee>()).Where(s => s != null && s.Id != null && s.FullName != null).ToList();

            ViewData["DepartmentId"] = new SelectList(departments, "Id", "Name", employee.DepartmentId);
            ViewData["JobTitleId"] = new SelectList(jobTitles, "Id", "Title", employee.JobTitleId);
            ViewData["SupervisorId"] = new SelectList(supervisors, "Id", "FullName", employee.SupervisorId);
            
            // Add tax settings for the view
            var taxSettings = await _context.TaxSettings
                .Where(t => t.Type == TaxType.IncomeTax && t.IsActive)
                .OrderBy(t => t.MinSalary)
                .ToListAsync();

            ViewBag.TaxSettings = taxSettings ?? new List<TaxSetting>();
            
            var pensionSetting = await _context.TaxSettings
                .FirstOrDefaultAsync(t => t.Type == TaxType.Pension && t.IsActive);

            ViewBag.Pension = pensionSetting ?? new TaxSetting 
            { 
                Type = TaxType.Pension,
                Percentage = 7, // Default pension rate
                IsActive = true
            };

            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Edit(string id, [Bind("Id,FirstName,LastName,Email,Salary,PhoneNumber,DepartmentId,JobTitleId,BasicSalary,UserId,SupervisorId,BadgeNumber")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing employee to preserve other fields
                    var existingEmployee = await _context.Employees
                        .Include(e => e.EmployeeTaxes)
                        .FirstOrDefaultAsync(e => e.Id == id);

                    if (existingEmployee == null)
                    {
                        return NotFound();
                    }

                    // Update only the fields that are bound
                    existingEmployee.FirstName = employee.FirstName;
                    existingEmployee.LastName = employee.LastName;
                    existingEmployee.Email = employee.Email;
                    existingEmployee.PhoneNumber = employee.PhoneNumber;
                    existingEmployee.DepartmentId = employee.DepartmentId;
                    existingEmployee.JobTitleId = employee.JobTitleId;
                    existingEmployee.BasicSalary = employee.BasicSalary;
                    existingEmployee.SupervisorId = employee.SupervisorId;
                    existingEmployee.Salary = employee.Salary;

                    // Calculate taxes using TaxSettings
                    var gross = employee.BasicSalary;
                    var taxSettingsForView = await _context.TaxSettings
                        .Where(t => t.IsActive && t.Type == TaxType.IncomeTax)
                        .OrderBy(t => t.MinSalary)
                        .ToListAsync();

                    // Find the applicable tax bracket
                    var applicableTax = taxSettingsForView
                        .Where(t => (t.MinSalary == null || gross >= t.MinSalary) && 
                                   (t.MaxSalary == null || gross <= t.MaxSalary))
                        .OrderByDescending(t => t.MinSalary)
                        .FirstOrDefault();

                    decimal totalTax = 0;
                    var taxPercentages = new Dictionary<string, decimal>();

                    if (applicableTax != null)
                    {
                        // Calculate income tax
                        var incomeTax = (gross * applicableTax.Percentage / 100m) - (applicableTax.Subtraction ?? 0);
                        totalTax += incomeTax > 0 ? incomeTax : 0;

                        // Add to taxPercentages for EmployeeTax table
                        taxPercentages["IncomeTax"] = applicableTax.Percentage;
                    }

                    // Calculate pension (7%)
                    var pensionRate = await _context.TaxSettings
                        .FirstOrDefaultAsync(t => t.Type == TaxType.Pension && t.IsActive) ?? 
                        new TaxSetting { Percentage = 7 }; // Default to 7% if not configured

                    var pensionTax = gross * (pensionRate.Percentage / 100m);
                    totalTax += pensionTax;
                    taxPercentages["Pension"] = pensionRate.Percentage;

                    // Calculate net salary
                    var net = gross - totalTax;
                    existingEmployee.Salary = net;

                    // Update employee
                    _context.Employees.Update(existingEmployee);

                    // Remove old taxes
                    var oldTaxes = _context.EmployeeTaxes.Where(t => t.EmployeeId == employee.Id);
                    _context.EmployeeTaxes.RemoveRange(oldTaxes);

                    // Add new taxes
                    foreach (var kvp in taxPercentages)
                    {
                        var empTax = new EmployeeTax 
                        { 
                            EmployeeId = employee.Id, 
                            TaxName = kvp.Key, 
                            Percentage = kvp.Value,
                            IsActive = true
                        };
                        _context.EmployeeTaxes.Add(empTax);
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Employee updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            ViewData["DepartmentId"] = new SelectList(
                await _context.Departments.ToListAsync() ?? new List<Department>(), 
                "Id", 
                "Name", 
                employee.DepartmentId
            );

            ViewData["JobTitleId"] = new SelectList(
                await _context.JobTitles.ToListAsync() ?? new List<JobTitle>(), 
                "Id", 
                "Title", 
                employee.JobTitleId
            );

            ViewData["SupervisorId"] = new SelectList(
                await _context.Employees
                    .Where(e => e.Id != employee.Id)
                    .ToListAsync() ?? new List<Employee>(), 
                "Id", 
                "FullName", 
                employee.SupervisorId
            );

            // Ensure tax settings and pension are set for the view
            var taxSettingsForViewInvalid = await _context.TaxSettings
                .Where(t => t.Type == TaxType.IncomeTax && t.IsActive)
                .OrderBy(t => t.MinSalary)
                .ToListAsync();
            ViewBag.TaxSettings = taxSettingsForViewInvalid ?? new List<TaxSetting>();

            var pensionSetting = await _context.TaxSettings
                .FirstOrDefaultAsync(t => t.Type == TaxType.Pension && t.IsActive);
            ViewBag.Pension = pensionSetting ?? new TaxSetting
            {
                Type = TaxType.Pension,
                Percentage = 7, // Default pension rate
                IsActive = true
            };

            return View(employee);
        }

        // GET: Employees/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.JobTitle)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // Add actions for managing EmployeeTaxes
        // GET: Employees/Taxes/{id}
        public async Task<IActionResult> Taxes(string id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees
                .Include(e => e.EmployeeTaxes)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // POST: Employees/AddTax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTax(string employeeId, string taxName, decimal percentage)
        {
            if (string.IsNullOrEmpty(employeeId) || string.IsNullOrEmpty(taxName)) return BadRequest();
            var tax = new EmployeeTax { EmployeeId = employeeId, TaxName = taxName, Percentage = percentage };
            _context.EmployeeTaxes.Add(tax);
            await _context.SaveChangesAsync();
            return RedirectToAction("Taxes", new { id = employeeId });
        }

        // POST: Employees/RemoveTax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTax(string taxId, string employeeId)
        {
            var tax = await _context.EmployeeTaxes.FindAsync(taxId);
            if (tax != null) _context.EmployeeTaxes.Remove(tax);
            await _context.SaveChangesAsync();
            return RedirectToAction("Taxes", new { id = employeeId });
        }

        private bool EmployeeExists(string id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
} 