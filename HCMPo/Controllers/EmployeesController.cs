using HCMPo.Data;
using HCMPo.Models;
using HCMPo.Models.ViewModels;
using HCMPo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace HCMPo.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EmployeesController> _logger;
        private readonly IUnifiedEmployeeService _unifiedEmployeeService;

        public EmployeesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<EmployeesController> logger,
            IUnifiedEmployeeService unifiedEmployeeService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _unifiedEmployeeService = unifiedEmployeeService;
        }

        // GET: Employees
        public async Task<IActionResult> Index(string searchTerm, string organizationUnitId, string jobTitleId, EmploymentStatus? status)
        {
            var query = _context.Employees
                .Include(e => e.OrganizationUnit)
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

            if (!string.IsNullOrEmpty(organizationUnitId))
            {
                query = query.Where(e => e.OrganizationUnitId == organizationUnitId);
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
                OrganizationUnitId = organizationUnitId,
                JobTitleId = jobTitleId,
                Status = status,
                Employees = await query.ToListAsync(),
                OrganizationUnits = new SelectList(_context.OrganizationUnits, "Id", "Name"),
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
                .Include(e => e.OrganizationUnit)
                .Include(e => e.JobTitle)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (employee == null)
            {
                _logger.LogWarning("Employee with ID: {EmployeeId} not found.", id);
                return NotFound();
            }

            // Fetch associated user and roles if ApplicationUserId is set
            if (!string.IsNullOrEmpty(employee.ApplicationUserId))
            {
                var user = await _userManager.FindByIdAsync(employee.ApplicationUserId);
                if (user != null)
                {
                    ViewBag.UserName = user.UserName;
                    ViewBag.UserEmail = user.Email;
                    ViewBag.UserRoles = await _userManager.GetRolesAsync(user);
                }
            }

            // Log the Salary value after loading from DB
            _logger.LogInformation("Loaded employee details. Salary value: {SalaryValue}", employee.Salary);

            return View(employee);
        }

        // GET: Employees/Create
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // If admin, show all org units, otherwise filter by user's org unit
            var query = _context.OrganizationUnits.AsQueryable();
            if (!isAdmin)
            {
                var userOrgUnitId = await _context.Employees
                    .Where(e => e.ApplicationUserId == userId)
                    .Select(e => e.OrganizationUnitId)
                    .FirstOrDefaultAsync();

                if (userOrgUnitId != null)
                {
                    query = query.Where(ou => ou.Id == userOrgUnitId);
                }
            }

            ViewData["OrganizationUnitId"] = new SelectList(
                await query.OrderBy(o => o.Name).ToListAsync(),
                "Id",
                "Name"
            );
            ViewData["JobTitleId"] = new SelectList(_context.JobTitles, "Id", "Title");
            ViewBag.IncomeTaxBrackets = _context.TaxSettings.Where(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "IncomeTax")).OrderBy(t => t.MinSalary).ToList();
            ViewBag.Pension = _context.TaxSettings.FirstOrDefault(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "Pension"));
            ViewBag.OtherTaxes = _context.TaxSettings.Where(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "Other")).ToList();
            ViewBag.DeductionTypes = _context.DeductionTypes.Where(d => d.IsActive).OrderBy(d => d.Order).ToList();
            // For Create, no applied taxes by default
            ViewBag.AppliedTaxes = new Dictionary<string, decimal>();
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,PhoneNumber,HireDate,EmergencyContact,EmergencyPhone,DateOfBirth,EmploymentDate,BasicSalary,Salary,OrganizationUnitId,JobTitleId,Status,Address,BadgeNumber,Gender,City,BankName,BankAccountNumber,TinNumber,Position,AmharicFirstName,AmharicLastName,SupervisorId")] Employee employee)
        {
            _logger.LogInformation("Create POST called");
            _logger.LogInformation("ModelState.IsValid: {IsValid}", ModelState.IsValid);
            
            // Handle Ethiopian date conversion
            var hireDateGregorian = Request.Form["HireDateGregorian"].ToString();
            var dateOfBirthGregorian = Request.Form["DateOfBirthGregorian"].ToString();
            var employmentDateGregorian = Request.Form["EmploymentDateGregorian"].ToString();

            if (!string.IsNullOrEmpty(hireDateGregorian) && DateTime.TryParse(hireDateGregorian, out var hireDate))
            {
                employee.HireDate = hireDate;
            }

            if (!string.IsNullOrEmpty(dateOfBirthGregorian) && DateTime.TryParse(dateOfBirthGregorian, out var dateOfBirth))
            {
                employee.DateOfBirth = dateOfBirth;
            }

            if (!string.IsNullOrEmpty(employmentDateGregorian) && DateTime.TryParse(employmentDateGregorian, out var employmentDate))
            {
                employee.EmploymentDate = employmentDate;
            }

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
                ViewData["OrganizationUnitId"] = new SelectList(_context.OrganizationUnits, "Id", "Name", employee.OrganizationUnitId);
                ViewData["JobTitleId"] = new SelectList(_context.JobTitles, "Id", "Title", employee.JobTitleId);
                ViewBag.IncomeTaxBrackets = _context.TaxSettings.Where(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "IncomeTax")).OrderBy(t => t.MinSalary).ToList();
                ViewBag.Pension = _context.TaxSettings.FirstOrDefault(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "Pension"));
                ViewBag.OtherTaxes = _context.TaxSettings.Where(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "Other")).ToList();
                ViewBag.DeductionTypes = _context.DeductionTypes.Where(d => d.IsActive).OrderBy(d => d.Order).ToList();
                // Pre-fill deduction values after validation error
                var deductionTypesForPrefillError1 = _context.DeductionTypes.Where(d => d.IsActive).ToList();
                var appliedTaxesForError1 = new Dictionary<string, decimal>();
                foreach (var dt in deductionTypesForPrefillError1)
                {
                    var isChecked = Request.Form[$"deduction_{dt.Id}"] == "on";
                    var valueStr = Request.Form[$"deductionValue_{dt.Id}"];
                    if (isChecked && decimal.TryParse(valueStr, out var value) && value > 0)
                    {
                        appliedTaxesForError1[dt.DisplayName] = value;
                    }
                }
                // Also handle Income Tax and Pension checkboxes
                if (Request.Form["incomeTax"] == "on" || Request.Form["taxes"].ToString().Contains("IncomeTax"))
                    appliedTaxesForError1["Income Tax"] = 1; // Just a flag for checked
                if (Request.Form["pension"] == "on" || Request.Form["taxes"].ToString().Contains("Pension"))
                    appliedTaxesForError1["Pension"] = 1;
                ViewBag.AppliedTaxes = appliedTaxesForError1;
                return View(employee);
            }

            var taxPercentages2 = new Dictionary<string, decimal>();
            foreach (var tax in selectedTaxes)
            {
                var percentStr = Request.Form[$"taxPercent_{tax.Replace(" ", "")}"];
                if (decimal.TryParse(percentStr, out var percent))
                    taxPercentages2[tax] = percent;
            }
            // Handle 'Other' tax name
            string otherTaxName = Request.Form["otherTaxDropdown"];
            if (selectedTaxes.Contains("Other") && !string.IsNullOrEmpty(otherTaxName))
            {
                var percentStr = Request.Form["taxPercent_Other"];
                if (decimal.TryParse(percentStr, out var percent))
                    taxPercentages2[otherTaxName] = percent;
                taxPercentages2.Remove("Other");
            }
            // Calculate net salary
            decimal gross2 = employee.BasicSalary;
            decimal totalTax = 0;
            foreach (var kvp in taxPercentages2)
            {
                if (kvp.Key == "Income Tax")
                {
                    var taxSettingsForView = _context.TaxSettings
                        .Where(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "IncomeTax"))
                        .OrderBy(t => t.MinSalary)
                        .ToList();
                    var applicableTax = taxSettingsForView
                        .Where(t => (t.MinSalary == null || gross2 >= t.MinSalary) &&
                                   (t.MaxSalary == null || gross2 <= t.MaxSalary))
                        .OrderByDescending(t => t.MinSalary)
                        .FirstOrDefault();
                    var subtraction = applicableTax?.Subtraction ?? 0;
                    totalTax += (gross2 * kvp.Value / 100m) - subtraction;
                }
                else if (kvp.Key == "Pension")
                {
                    totalTax += (gross2 * kvp.Value / 100m);
                }
                else
                {
                    totalTax += kvp.Value;
                }
            }
            decimal net = gross2 - totalTax;
            employee.Salary = net;

            if (ModelState.IsValid)
            {
                try
                {
                    employee.Id = Guid.NewGuid().ToString();
                    _context.Employees.Add(employee);
                    _logger.LogInformation("About to call SaveChangesAsync (employee)");
                    await _context.SaveChangesAsync();
                    // Sync to attendance system
                    var attSyncResult = await _unifiedEmployeeService.SyncEmployeeToAttDbAsync(employee);
                    if (!attSyncResult)
                    {
                        _logger.LogWarning("Failed to sync employee {EmployeeId} to att_db", employee.Id);
                        TempData["WarningMessage"] = "Employee created, but failed to sync to attendance system.";
                    }
                    // Save EmployeeTax records
                    var taxPercentagesForError = new Dictionary<string, decimal>();
                    if ((Request.Form["incomeTax"] == "on") || (Request.Form["taxes"].ToString().Contains("IncomeTax")))
                    {
                        var taxSettingsForView = _context.TaxSettings
                            .Where(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "IncomeTax"))
                            .OrderBy(t => t.MinSalary)
                            .ToList();
                        var grossForError = employee.BasicSalary;
                        var applicableTax = taxSettingsForView
                            .Where(t => (t.MinSalary == null || grossForError >= t.MinSalary) &&
                                       (t.MaxSalary == null || grossForError <= t.MaxSalary))
                            .OrderByDescending(t => t.MinSalary)
                            .FirstOrDefault();
                        if (applicableTax != null)
                        {
                            taxPercentagesForError["Income Tax"] = applicableTax.Percentage;
                        }
                    }
                    if ((Request.Form["pension"] == "on") || (Request.Form["taxes"].ToString().Contains("Pension")))
                    {
                        var pensionRate = _context.TaxSettings
                            .FirstOrDefault(t => t.Type == (TaxType)Enum.Parse(typeof(TaxType), "Pension") && t.IsActive) ?? new TaxSetting { Percentage = 7 };
                        taxPercentagesForError["Pension"] = pensionRate.Percentage;
                    }
                    var deductionTypes = _context.DeductionTypes.Where(d => d.IsActive).ToList();
                    foreach (var dt in deductionTypes)
                    {
                        var isChecked = Request.Form[$"deduction_{dt.Id}"] == "on";
                        var valueStr = Request.Form[$"deductionValue_{dt.Id}"];
                        var existingTax = await _context.EmployeeTaxes.FirstOrDefaultAsync(t => t.EmployeeId == employee.Id && t.TaxName == dt.DisplayName);
                        if (isChecked && decimal.TryParse(valueStr, out var value) && value > 0)
                        {
                            if (existingTax != null)
                            {
                                existingTax.Percentage = value;
                                existingTax.IsApplied = true;
                                _context.EmployeeTaxes.Update(existingTax);
                            }
                            else
                            {
                                var empTax = new EmployeeTax
                                {
                                    EmployeeId = employee.Id,
                                    TaxName = dt.DisplayName,
                                    Percentage = value,
                                    IsActive = true,
                                    IsApplied = true
                                };
                                _context.EmployeeTaxes.Add(empTax);
                            }
                        }
                        else
                        {
                            if (existingTax != null)
                            {
                                existingTax.IsApplied = false;
                                _context.EmployeeTaxes.Update(existingTax);
                            }
                        }
                    }
                    foreach (var kvp in taxPercentagesForError)
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

            ViewData["OrganizationUnitId"] = new SelectList(_context.OrganizationUnits, "Id", "Name");
            ViewData["JobTitleId"] = new SelectList(_context.JobTitles, "Id", "Title", employee.JobTitleId);
            ViewBag.IncomeTaxBrackets = _context.TaxSettings.Where(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "IncomeTax")).OrderBy(t => t.MinSalary).ToList();
            ViewBag.Pension = _context.TaxSettings.FirstOrDefault(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "Pension"));
            ViewBag.OtherTaxes = _context.TaxSettings.Where(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "Other")).ToList();
            ViewBag.DeductionTypes = _context.DeductionTypes.Where(d => d.IsActive).OrderBy(d => d.Order).ToList();
            // Pre-fill deduction values after validation error
            var deductionTypesForPrefillError2 = _context.DeductionTypes.Where(d => d.IsActive).ToList();
            var appliedTaxesForError2 = new Dictionary<string, decimal>();
            foreach (var dt in deductionTypesForPrefillError2)
            {
                var isChecked = Request.Form[$"deduction_{dt.Id}"] == "on";
                var valueStr = Request.Form[$"deductionValue_{dt.Id}"];
                if (isChecked && decimal.TryParse(valueStr, out var value) && value > 0)
                {
                    appliedTaxesForError2[dt.DisplayName] = value;
                }
            }
            // Also handle Income Tax and Pension checkboxes
            if (Request.Form["incomeTax"] == "on" || Request.Form["taxes"].ToString().Contains("IncomeTax"))
                appliedTaxesForError2["Income Tax"] = 1; // Just a flag for checked
            if (Request.Form["pension"] == "on" || Request.Form["taxes"].ToString().Contains("Pension"))
                appliedTaxesForError2["Pension"] = 1;
            ViewBag.AppliedTaxes = appliedTaxesForError2;
            return View(employee);
        }

        // GET: Employees/Edit/5
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.OrganizationUnit)
                .Include(e => e.JobTitle)
                .Include(e => e.Supervisor)
                .Include(e => e.EmployeeTaxes)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            // Populate view bags for dropdowns
            ViewBag.JobTitles = new SelectList(await _context.JobTitles.Where(jt => jt.IsActive).ToListAsync(), "Id", "Title", employee.JobTitleId);
            ViewBag.OrganizationUnits = new SelectList(await _context.OrganizationUnits.Where(ou => ou.IsActive).ToListAsync(), "Id", "Name", employee.OrganizationUnitId);
            
            var employeesList = await _context.Employees.Where(e => e.IsActive && e.Id != id).ToListAsync();
            ViewBag.Supervisors = new SelectList(employeesList, "Id", "FullName", employee.SupervisorId);
            
            // Populate tax settings
            ViewBag.IncomeTaxBrackets = await _context.TaxSettings.Where(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "IncomeTax")).OrderBy(t => t.MinSalary).ToListAsync();
            ViewBag.Pension = await _context.TaxSettings.FirstOrDefaultAsync(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "Pension"));
            ViewBag.DeductionTypes = await _context.DeductionTypes.Where(d => d.IsActive).ToListAsync();

            // Populate applied taxes for the employee
            var appliedTaxes = new Dictionary<string, decimal>();
            foreach (var tax in employee.EmployeeTaxes.Where(t => t.IsApplied))
            {
                appliedTaxes[tax.TaxName] = tax.Percentage;
            }
            ViewBag.AppliedTaxes = appliedTaxes;

            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Edit(string id, [Bind("Id,FirstName,LastName,Email,Salary,PhoneNumber,OrganizationUnitId,JobTitleId,BasicSalary,UserId,SupervisorId,BadgeNumber,Gender,Address,City,BankName,BankAccountNumber,TinNumber,Position,Status,DateOfBirth,HireDate,EmploymentDate,AmharicFirstName,AmharicLastName")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            // Handle Ethiopian date conversion
            var hireDateGregorian = Request.Form["HireDateGregorian"].ToString();
            var dateOfBirthGregorian = Request.Form["DateOfBirthGregorian"].ToString();
            var employmentDateGregorian = Request.Form["EmploymentDateGregorian"].ToString();

            if (!string.IsNullOrEmpty(hireDateGregorian) && DateTime.TryParse(hireDateGregorian, out var hireDate))
            {
                employee.HireDate = hireDate;
            }

            if (!string.IsNullOrEmpty(dateOfBirthGregorian) && DateTime.TryParse(dateOfBirthGregorian, out var dateOfBirth))
            {
                employee.DateOfBirth = dateOfBirth;
            }

            if (!string.IsNullOrEmpty(employmentDateGregorian) && DateTime.TryParse(employmentDateGregorian, out var employmentDate))
            {
                employee.EmploymentDate = employmentDate;
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

                    // Update all the fields
                    existingEmployee.FirstName = employee.FirstName;
                    existingEmployee.LastName = employee.LastName;
                    existingEmployee.AmharicFirstName = employee.AmharicFirstName;
                    existingEmployee.AmharicLastName = employee.AmharicLastName;
                    existingEmployee.Email = employee.Email;
                    existingEmployee.PhoneNumber = employee.PhoneNumber;
                    existingEmployee.OrganizationUnitId = employee.OrganizationUnitId;
                    existingEmployee.JobTitleId = employee.JobTitleId;
                    existingEmployee.BasicSalary = employee.BasicSalary;
                    existingEmployee.SupervisorId = employee.SupervisorId;
                    existingEmployee.Salary = employee.Salary;
                    existingEmployee.Gender = employee.Gender;
                    existingEmployee.Address = employee.Address;
                    existingEmployee.City = employee.City;
                    existingEmployee.BankName = employee.BankName;
                    existingEmployee.BankAccountNumber = employee.BankAccountNumber;
                    existingEmployee.TinNumber = employee.TinNumber;
                    existingEmployee.Position = employee.Position;
                    existingEmployee.Status = employee.Status;
                    existingEmployee.DateOfBirth = employee.DateOfBirth;
                    existingEmployee.HireDate = employee.HireDate;
                    existingEmployee.EmploymentDate = employee.EmploymentDate;
                    existingEmployee.ModifiedAt = DateTime.UtcNow;
                    existingEmployee.ModifiedBy = User.Identity.Name;

                    // Remove old taxes
                    var oldTaxes = _context.EmployeeTaxes.Where(t => t.EmployeeId == employee.Id);
                    _context.EmployeeTaxes.RemoveRange(oldTaxes);

                    // Read selected taxes from form
                    var selectedTaxes = Request.Form["taxes"].ToList();
                    var taxPercentages2 = new Dictionary<string, decimal>();

                    // Income Tax
                    if ((Request.Form["incomeTax"] == "on") || (Request.Form["taxes"].ToString().Contains("IncomeTax")))
                    {
                        var taxSettingsForView = await _context.TaxSettings
                            .Where(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "IncomeTax"))
                            .OrderBy(t => t.MinSalary)
                            .ToListAsync();
                        var gross2 = employee.BasicSalary;
                        var applicableTax = taxSettingsForView
                            .Where(t => (t.MinSalary == null || gross2 >= t.MinSalary) &&
                                       (t.MaxSalary == null || gross2 <= t.MaxSalary))
                            .OrderByDescending(t => t.MinSalary)
                            .FirstOrDefault();
                        if (applicableTax != null)
                        {
                            taxPercentages2["Income Tax"] = applicableTax.Percentage;
                        }
                    }

                    // Pension
                    if ((Request.Form["pension"] == "on") || (Request.Form["taxes"].ToString().Contains("Pension")))
                    {
                        var pensionRate = await _context.TaxSettings
                            .FirstOrDefaultAsync(t => t.Type == (TaxType)Enum.Parse(typeof(TaxType), "Pension") && t.IsActive) ?? new TaxSetting { Percentage = 7 };
                        taxPercentages2["Pension"] = pensionRate.Percentage;
                    }

                    // Other deductions
                    var deductionTypes = await _context.DeductionTypes.Where(d => d.IsActive).ToListAsync();
                    foreach (var dt in deductionTypes)
                    {
                        var isChecked = Request.Form[$"deduction_{dt.Id}"] == "on";
                        var valueStr = Request.Form[$"deductionValue_{dt.Id}"];
                        var existingTax = await _context.EmployeeTaxes.FirstOrDefaultAsync(t => t.EmployeeId == employee.Id && t.TaxName == dt.DisplayName);
                        if (isChecked && decimal.TryParse(valueStr, out var value) && value > 0)
                        {
                            if (existingTax != null)
                            {
                                existingTax.Percentage = value;
                                existingTax.IsApplied = true;
                                _context.EmployeeTaxes.Update(existingTax);
                            }
                            else
                            {
                                var empTax = new EmployeeTax
                                {
                                    EmployeeId = employee.Id,
                                    TaxName = dt.DisplayName,
                                    Percentage = value,
                                    IsActive = true,
                                    IsApplied = true
                                };
                                _context.EmployeeTaxes.Add(empTax);
                            }
                        }
                        else
                        {
                            if (existingTax != null)
                            {
                                existingTax.IsApplied = false;
                                _context.EmployeeTaxes.Update(existingTax);
                            }
                        }
                    }

                    // Add new taxes (only those checked)
                    foreach (var kvp in taxPercentages2)
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

                    // Calculate net salary
                    decimal gross = existingEmployee.BasicSalary;
                    decimal totalTax = 0;
                    foreach (var kvp in taxPercentages2)
                    {
                        if (kvp.Key == "Income Tax")
                        {
                            var taxSettingsForView = await _context.TaxSettings
                                .Where(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "IncomeTax"))
                                .OrderBy(t => t.MinSalary)
                                .ToListAsync();
                            var applicableTax = taxSettingsForView
                                .Where(t => (t.MinSalary == null || gross >= t.MinSalary) &&
                                           (t.MaxSalary == null || gross <= t.MaxSalary))
                                .OrderByDescending(t => t.MinSalary)
                                .FirstOrDefault();
                            var subtraction = applicableTax?.Subtraction ?? 0;
                            totalTax += (gross * kvp.Value / 100m) - subtraction;
                        }
                        else if (kvp.Key == "Pension")
                        {
                            totalTax += (gross * kvp.Value / 100m);
                        }
                        else
                        {
                            totalTax += kvp.Value;
                        }
                    }
                    decimal net = gross - totalTax;
                    existingEmployee.Salary = net;

                    await _context.SaveChangesAsync();
                    
                    // Sync changes to Att_db
                    await _unifiedEmployeeService.SyncEmployeeToAttDbAsync(existingEmployee);
                    
                    TempData["SuccessMessage"] = "Employee updated successfully and synced to attendance system.";
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating employee {Id}", id);
                    ModelState.AddModelError("", "An error occurred while updating the employee.");
                }
            }

            // If we got this far, something failed, redisplay form
            await LoadCreateEditViewData();
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
                .Include(e => e.OrganizationUnit)
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

        // GET: Employees/Taxes/{id}
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Taxes(string id)
        {
            if (id == null) return NotFound();
            
            var employee = await _context.Employees
                .Include(e => e.EmployeeTaxes)
                .FirstOrDefaultAsync(e => e.Id == id);
                
            if (employee == null) return NotFound();

            // Get tax settings for reference
            ViewBag.IncomeTaxBrackets = await _context.TaxSettings
                .Where(t => t.Type == (TaxType)Enum.Parse(typeof(TaxType), "IncomeTax") && t.IsActive)
                .OrderBy(t => t.MinSalary)
                .ToListAsync();
                
            ViewBag.Pension = await _context.TaxSettings
                .FirstOrDefaultAsync(t => t.Type == (TaxType)Enum.Parse(typeof(TaxType), "Pension") && t.IsActive);
                
            ViewBag.OtherTaxes = await _context.TaxSettings
                .Where(t => t.Type == (TaxType)Enum.Parse(typeof(TaxType), "Other") && t.IsActive)
                .ToListAsync();

            return View(employee);
        }

        // POST: Employees/AddTax
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> AddTax(string employeeId, string taxName, decimal percentage)
        {
            if (string.IsNullOrEmpty(employeeId) || string.IsNullOrEmpty(taxName))
            {
                TempData["ErrorMessage"] = "Invalid tax information provided.";
                return RedirectToAction("Taxes", new { id = employeeId });
            }

            try
            {
                // Validate percentage
                if (percentage <= 0 || percentage > 100)
                {
                    TempData["ErrorMessage"] = "Tax percentage must be between 0 and 100.";
                    return RedirectToAction("Taxes", new { id = employeeId });
                }

                var employee = await _context.Employees
                    .Include(e => e.EmployeeTaxes)
                    .FirstOrDefaultAsync(e => e.Id == employeeId);

                if (employee == null)
                {
                    return NotFound();
                }

                // Check if tax already exists
                var existingTax = employee.EmployeeTaxes
                    .FirstOrDefault(t => t.TaxName == taxName && t.IsActive);

                if (existingTax != null)
                {
                    existingTax.Percentage = percentage;
                    existingTax.IsApplied = true;
                    _context.EmployeeTaxes.Update(existingTax);
                }
                else
                {
                    var tax = new EmployeeTax
                    {
                        EmployeeId = employeeId,
                        TaxName = taxName,
                        Percentage = percentage,
                        IsActive = true,
                        IsApplied = true
                    };
                    _context.EmployeeTaxes.Add(tax);
                }

                // Recalculate net salary
                decimal gross = employee.BasicSalary;
                decimal totalTax = 0;

                foreach (var tax in employee.EmployeeTaxes.Where(t => t.IsApplied))
                {
                    if (tax.TaxName == "Income Tax")
                    {
                        var taxSettings = await _context.TaxSettings
                            .Where(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "IncomeTax"))
                            .OrderBy(t => t.MinSalary)
                            .ToListAsync();

                        var applicableTax = taxSettings
                            .Where(t => (t.MinSalary == null || gross >= t.MinSalary) &&
                                       (t.MaxSalary == null || gross <= t.MaxSalary))
                            .OrderByDescending(t => t.MinSalary)
                            .FirstOrDefault();

                        var subtraction = applicableTax?.Subtraction ?? 0;
                        totalTax += (gross * tax.Percentage / 100m) - subtraction;
                    }
                    else if (tax.TaxName == "Pension")
                    {
                        totalTax += (gross * tax.Percentage / 100m);
                    }
                    else
                    {
                        totalTax += tax.Percentage;
                    }
                }

                employee.Salary = gross - totalTax;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tax added successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tax for employee {EmployeeId}", employeeId);
                TempData["ErrorMessage"] = "An error occurred while adding the tax.";
            }

            return RedirectToAction("Taxes", new { id = employeeId });
        }

        // POST: Employees/RemoveTax
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> RemoveTax(string taxId, string employeeId)
        {
            try
            {
                var tax = await _context.EmployeeTaxes.FindAsync(taxId);
                if (tax != null)
                {
                    // Instead of removing, mark as inactive
                    tax.IsApplied = false;
                    _context.EmployeeTaxes.Update(tax);

                    // Recalculate net salary
                    var employee = await _context.Employees
                        .Include(e => e.EmployeeTaxes)
                        .FirstOrDefaultAsync(e => e.Id == employeeId);

                    if (employee != null)
                    {
                        decimal gross = employee.BasicSalary;
                        decimal totalTax = 0;

                        foreach (var activeTax in employee.EmployeeTaxes.Where(t => t.IsApplied))
                        {
                            if (activeTax.TaxName == "Income Tax")
                            {
                                var taxSettings = await _context.TaxSettings
                                    .Where(t => t.IsActive && t.Type == (TaxType)Enum.Parse(typeof(TaxType), "IncomeTax"))
                                    .OrderBy(t => t.MinSalary)
                                    .ToListAsync();

                                var applicableTax = taxSettings
                                    .Where(t => (t.MinSalary == null || gross >= t.MinSalary) &&
                                               (t.MaxSalary == null || gross <= t.MaxSalary))
                                    .OrderByDescending(t => t.MinSalary)
                                    .FirstOrDefault();

                                var subtraction = applicableTax?.Subtraction ?? 0;
                                totalTax += (gross * activeTax.Percentage / 100m) - subtraction;
                            }
                            else if (activeTax.TaxName == "Pension")
                            {
                                totalTax += (gross * activeTax.Percentage / 100m);
                            }
                            else
                            {
                                totalTax += activeTax.Percentage;
                            }
                        }

                        employee.Salary = gross - totalTax;
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Tax removed successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing tax {TaxId} for employee {EmployeeId}", taxId, employeeId);
                TempData["ErrorMessage"] = "An error occurred while removing the tax.";
            }

            return RedirectToAction(nameof(Taxes), new { id = employeeId });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> CheckBadgeNumber(string badgeNumber, string excludeEmployeeId = null)
        {
            if (string.IsNullOrWhiteSpace(badgeNumber))
            {
                return Json(new { isAvailable = false, message = "Badge number cannot be empty." });
            }

            var query = _context.Employees.AsQueryable();

            if (!string.IsNullOrEmpty(excludeEmployeeId))
            {
                query = query.Where(e => e.Id != excludeEmployeeId);
            }

            var employeeExists = await query.AnyAsync(e => e.BadgeNumber == badgeNumber);

            if (employeeExists)
            {
                return Json(new { isAvailable = false, message = "Badge number already exists." });
            }

            return Json(new { isAvailable = true });
        }
        
        private async Task LoadCreateEditViewData()
        {
            ViewBag.OrganizationUnits = new SelectList(await _context.OrganizationUnits.Where(ou => ou.IsActive).ToListAsync(), "Id", "Name");
            ViewBag.JobTitles = new SelectList(await _context.JobTitles.Where(jt => jt.IsActive).ToListAsync(), "Id", "Title");
            ViewBag.Supervisors = new SelectList(await _context.Employees.Where(e => e.IsActive).ToListAsync(), "Id", "FullName");
            ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(EmploymentStatus)));
        }

        [HttpGet]
        public async Task<IActionResult> GetJobTitlesByOrgUnit(string orgUnitId)
        {
            if (string.IsNullOrEmpty(orgUnitId))
                return Json(new List<SelectListItem>());

            // Get the parent org unit ID
            var parentOrgUnitId = await _context.OrganizationUnits
                .Where(ou => ou.Id == orgUnitId)
                .Select(ou => ou.ParentId)
                .FirstOrDefaultAsync();

            // If no parent, use the selected org unit itself
            var targetOrgUnitId = parentOrgUnitId ?? orgUnitId;

            // Get job titles for the target org unit
            var jobTitles = await _context.JobTitles
                .Where(jt => jt.OrganizationUnitId == targetOrgUnitId && jt.IsActive)
                .OrderBy(jt => jt.Title)
                .Select(jt => new SelectListItem
                {
                    Value = jt.Id,
                    Text = jt.Title
                })
                .ToListAsync();

            return Json(jobTitles);
        }

        private bool EmployeeExists(string id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
} 