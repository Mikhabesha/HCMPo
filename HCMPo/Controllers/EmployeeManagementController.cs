using HCMPo.Data;
using HCMPo.Models;
using HCMPo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HCMPo.Controllers
{
    [Authorize]
    public class EmployeeManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnifiedEmployeeService _unifiedEmployeeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EmployeeManagementController> _logger;
        private readonly IUserEmployeeLinkService _linkService;

        public EmployeeManagementController(
            ApplicationDbContext context,
            IUnifiedEmployeeService unifiedEmployeeService,
            UserManager<ApplicationUser> userManager,
            ILogger<EmployeeManagementController> logger,
            IUserEmployeeLinkService linkService)
        {
            _context = context;
            _unifiedEmployeeService = unifiedEmployeeService;
            _userManager = userManager;
            _logger = logger;
            _linkService = linkService;
        }

        // GET: EmployeeManagement
        public async Task<IActionResult> Index(string searchString, string organizationUnitFilter, string statusFilter)
        {
            try
            {
                var employees = await _unifiedEmployeeService.GetUnifiedEmployeesAsync();

                // Apply filters
                if (!string.IsNullOrEmpty(searchString))
                {
                    employees = employees.Where(e => 
                        e.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        e.BadgeNumber.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        e.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrEmpty(organizationUnitFilter))
                {
                    employees = employees.Where(e => e.OrganizationUnitId == organizationUnitFilter).ToList();
                }

                if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<EmploymentStatus>(statusFilter, out var status))
                {
                    employees = employees.Where(e => e.Status == status).ToList();
                }

                // Load filter options
                ViewBag.OrganizationUnits = await _context.OrganizationUnits
                    .Where(ou => ou.IsActive)
                    .Select(ou => new SelectListItem { Value = ou.Id, Text = ou.Name })
                    .ToListAsync();

                ViewBag.Statuses = Enum.GetValues<EmploymentStatus>()
                    .Select(s => new SelectListItem { Value = s.ToString(), Text = s.ToString() })
                    .ToList();

                ViewBag.SearchString = searchString;
                ViewBag.OrganizationUnitFilter = organizationUnitFilter;
                ViewBag.StatusFilter = statusFilter;

                return View(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employees");
                TempData["ErrorMessage"] = "Error loading employees. Please try again.";
                return View(new List<Employee>());
            }
        }

        // GET: EmployeeManagement/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _unifiedEmployeeService.GetUnifiedEmployeeAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: EmployeeManagement/Create
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Create()
        {
            await LoadCreateEditViewData();
            return View();
        }

        // POST: EmployeeManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,HireDate,Salary,BadgeNumber,JobTitleId,OrganizationUnitId,PhoneNumber,BasicSalary,EmploymentDate,Status,Address,City,BankName,BankAccountNumber,TinNumber,DateOfBirth,Gender,SupervisorId,Position,AmharicFirstName,AmharicLastName")] Employee employee)
        {
            try
            {
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
                    // Check if badge number already exists
                    var existingEmployee = await _context.Employees
                        .FirstOrDefaultAsync(e => e.BadgeNumber == employee.BadgeNumber);
                    if (existingEmployee != null)
                    {
                        ModelState.AddModelError("BadgeNumber", "Badge number already exists.");
                        await LoadCreateEditViewData();
                        return View(employee);
                    }

                    // Set audit fields
                    employee.Id = Guid.NewGuid().ToString();
                    employee.CreatedAt = DateTime.UtcNow;
                    employee.CreatedBy = User.Identity.Name;
                    employee.IsActive = true;

                    // Use unified service to create employee (will auto-sync to Att_db)
                    await _unifiedEmployeeService.CreateEmployeeAsync(employee);

                    TempData["SuccessMessage"] = $"Employee {employee.FullName} created successfully and synced to attendance system!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee {FullName}", employee.FullName);
                ModelState.AddModelError("", "An error occurred while creating the employee.");
            }

            await LoadCreateEditViewData();
            return View(employee);
        }

        // GET: EmployeeManagement/Edit/5
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }
            
            await LoadCreateEditViewData(employee);
            
            return View(employee);
        }

        // POST: EmployeeManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Edit(string id, [Bind("Id,FirstName,LastName,Email,Salary,BadgeNumber,JobTitleId,OrganizationUnitId,PhoneNumber,BasicSalary,EmploymentDate,Status,Address,City,BankName,BankAccountNumber,TinNumber,DateOfBirth,Gender,SupervisorId,Position,AmharicFirstName,AmharicLastName,CreatedAt,CreatedBy,ApplicationUserId,UserId")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            // Handle Ethiopian date conversion
            var hireDateGregorian = Request.Form["HireDateGregorian"].ToString();
            var dateOfBirthGregorian = Request.Form["DateOfBirthGregorian"].ToString();
            var employmentDateGregorian = Request.Form["EmploymentDateGregorian"].ToString();

            if (DateTime.TryParse(hireDateGregorian, out var hireDate))
            {
                employee.HireDate = hireDate;
            }

            if (DateTime.TryParse(dateOfBirthGregorian, out var dateOfBirth))
            {
                employee.DateOfBirth = dateOfBirth;
            }

            if (DateTime.TryParse(employmentDateGregorian, out var employmentDate))
            {
                employee.EmploymentDate = employmentDate;
            }

            // Remove ModelState errors for dates since we are handling them manually
            ModelState.Remove("HireDate");
            ModelState.Remove("DateOfBirth");
            ModelState.Remove("EmploymentDate");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
                    if (existingEmployee == null)
                    {
                        return NotFound();
                    }

                    // Check for badge number uniqueness
                    var duplicateBadge = await _context.Employees
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.BadgeNumber == employee.BadgeNumber && e.Id != id);

                    if (duplicateBadge != null)
                    {
                        ModelState.AddModelError("BadgeNumber", "Badge number already exists.");
                        await LoadCreateEditViewData(employee);
                        return View(employee);
                    }
                    
                    // Map updated properties from the bound model to the existing entity
                    existingEmployee.FirstName = employee.FirstName;
                    existingEmployee.LastName = employee.LastName;
                    existingEmployee.AmharicFirstName = employee.AmharicFirstName;
                    existingEmployee.AmharicLastName = employee.AmharicLastName;
                    existingEmployee.Email = employee.Email;
                    existingEmployee.PhoneNumber = employee.PhoneNumber;
                    existingEmployee.Gender = employee.Gender;
                    existingEmployee.DateOfBirth = employee.DateOfBirth;
                    existingEmployee.BadgeNumber = employee.BadgeNumber;
                    existingEmployee.OrganizationUnitId = employee.OrganizationUnitId;
                    existingEmployee.JobTitleId = employee.JobTitleId;
                    existingEmployee.SupervisorId = employee.SupervisorId;
                    existingEmployee.HireDate = employee.HireDate;
                    existingEmployee.EmploymentDate = employee.EmploymentDate;
                    existingEmployee.BasicSalary = employee.BasicSalary;
                    existingEmployee.Salary = employee.Salary; // This should likely be calculated, not bound
                    existingEmployee.Status = employee.Status;
                    existingEmployee.Address = employee.Address;
                    existingEmployee.City = employee.City;
                    existingEmployee.Position = employee.Position;
                    existingEmployee.BankName = employee.BankName;
                    existingEmployee.BankAccountNumber = employee.BankAccountNumber;
                    existingEmployee.TinNumber = employee.TinNumber;

                    existingEmployee.ModifiedAt = DateTime.UtcNow;
                    existingEmployee.ModifiedBy = User.Identity?.Name;
                    
                    _context.Update(existingEmployee);
                    await _context.SaveChangesAsync();

                    // Sync changes to Att_db
                    await _unifiedEmployeeService.SyncEmployeeToAttDbAsync(existingEmployee);

                    TempData["SuccessMessage"] = $"Employee {existingEmployee.FullName} updated successfully!";
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
                        _logger.LogError("Concurrency exception for employee {EmployeeId}", employee.Id);
                        ModelState.AddModelError("", "The record you attempted to edit was modified by another user. Please go back and try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating employee {EmployeeId}", employee.Id);
                    ModelState.AddModelError("", "An unexpected error occurred while saving. Please try again.");
                }
            }
            
            // If we got this far, something failed, redisplay form
            await LoadCreateEditViewData(employee);
            return View(employee);
        }

        // GET: EmployeeManagement/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _unifiedEmployeeService.GetUnifiedEmployeeAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: EmployeeManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee != null)
                {
                    // Soft delete - just mark as inactive
                    employee.IsActive = false;
                    employee.ModifiedAt = DateTime.UtcNow;
                    employee.ModifiedBy = User.Identity.Name;
                    
                    _context.Employees.Update(employee);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Employee {employee.FullName} deactivated successfully!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the employee.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Create User Account for Employee
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUserAccount(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                return Json(new { success = false, message = "Employee ID is required" });
            }

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found" });
            }

            if (!string.IsNullOrEmpty(employee.ApplicationUserId))
            {
                return Json(new { success = false, message = "Employee already has a user account" });
            }

            // Generate user details
            var email = string.IsNullOrWhiteSpace(employee.Email) ? $"{employee.BadgeNumber}@hcm.com" : employee.Email;
            var userName = email;
            var tempPassword = $"{employee.FirstName?.ToLower().Trim()}@{employee.BadgeNumber}";

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                // If user exists but is not linked, link them
                if (string.IsNullOrEmpty(existingUser.EmployeeId))
                {
                    var linkResult = await _linkService.LinkUserToEmployeeAsync(existingUser.Id, employee.Id);
                    if (linkResult.success)
                        return Json(new { success = true, message = "Existing user linked successfully." });
                    else
                        return Json(new { success = false, message = $"Error linking existing user: {linkResult.message}" });
                }
                return Json(new { success = false, message = "A user with this email already exists and is linked to another employee." });
            }
            
            // Create new user
            var newUser = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmployeeId = employee.Id,
                EmailConfirmed = true 
            };

            var result = await _userManager.CreateAsync(newUser, tempPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation("User account created for {EmployeeName} with temp password {Password}", employee.FullName, tempPassword);
                
                // Link employee record to the new user
                employee.ApplicationUserId = newUser.Id;
                employee.UserId = newUser.Id; // Backward compatibility
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();

                return Json(new { success = true, tempPassword });
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Error creating user for {EmployeeName}: {Errors}", employee.FullName, errors);
                return Json(new { success = false, message = $"Error creating user account: {errors}" });
            }
        }

        // GET: API endpoint for DataTables
        [HttpGet]
        public async Task<IActionResult> GetEmployeesData()
        {
            try
            {
                var employees = await _unifiedEmployeeService.GetUnifiedEmployeesAsync();
                
                var data = employees.Select(e => new
                {
                    id = e.Id,
                    badgeNumber = e.BadgeNumber,
                    fullName = e.FullName,
                    email = e.Email,
                    phoneNumber = e.PhoneNumber,
                    organizationUnit = e.OrganizationUnit?.Name ?? "N/A",
                    jobTitle = e.JobTitle?.Title ?? "N/A",
                    status = e.Status.ToString(),
                    hasUserAccount = e.ApplicationUser != null,
                    hireDate = e.HireDate.ToString("MMM dd, yyyy")
                });

                return Json(new { data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employees data");
                return Json(new { data = new List<object>() });
            }
        }

        private async Task LoadCreateEditViewData(Employee employee = null)
        {
            var organizationUnits = await _context.OrganizationUnits
                .Where(ou => ou.IsActive)
                .OrderBy(ou => ou.Name)
                .ToListAsync();

            var jobTitles = await _context.JobTitles
                .Where(j => j.IsActive)
                .OrderBy(j => j.Title)
                .ToListAsync();
            
            var supervisors = await _context.Employees
                .Where(e => e.IsActive && (employee == null || e.Id != employee.Id))
                .OrderBy(e => e.FullName)
                .ToListAsync();

            ViewBag.OrganizationUnits = new SelectList(organizationUnits, "Id", "Name", employee?.OrganizationUnitId);
            ViewBag.JobTitles = new SelectList(jobTitles, "Id", "Title", employee?.JobTitleId);
            ViewBag.Supervisors = new SelectList(supervisors, "Id", "FullName", employee?.SupervisorId);
            ViewBag.Statuses = new SelectList(Enum.GetValues<EmploymentStatus>().Cast<EmploymentStatus>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList(), "Value", "Text", employee?.Status);
        }

        private bool EmployeeExists(string id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnlinkUserFromEmployee(string userId, string employeeId)
        {
            var (success, message) = await _linkService.UnlinkUserFromEmployeeAsync(userId, employeeId);
            return Json(new { success, message });
        }

        #if DEBUG || ADMIN
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MergeDuplicateEmployees()
        {
            try
            {
                var employees = await _context.Employees.ToListAsync();
                var employeesByEmail = employees
                    .GroupBy(e => e.Email)
                    .Where(g => g.Count() > 1)
                    .ToList();
                int merged = 0, removed = 0;
                foreach (var group in employeesByEmail)
                {
                    var correct = group.OrderByDescending(e => e.BadgeNumber.Length).First();
                    foreach (var dup in group.Where(e => e.Id != correct.Id))
                    {
                        var payrolls = await _context.Payrolls.Where(p => p.EmployeeId == dup.Id).ToListAsync();
                        foreach (var p in payrolls) p.EmployeeId = correct.Id;
                        var attendances = await _context.Attendances.Where(a => a.EmployeeId == dup.Id).ToListAsync();
                        foreach (var a in attendances) a.EmployeeId = correct.Id;
                        _context.Employees.Remove(dup);
                        removed++;
                    }
                    merged++;
                }
                await _context.SaveChangesAsync();
                return Json(new { merged, removed, message = $"Merged {merged} groups, removed {removed} duplicates." });
            }
            catch (Exception ex)
            {
                return Json(new { merged = 0, removed = 0, message = "Error: " + ex.Message });
            }
        }
        #endif

        // GET: Check Badge Number Availability
        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> CheckBadgeNumber(string badgeNumber, string excludeEmployeeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(badgeNumber))
                {
                    return Json(new { isAvailable = false, message = "Badge number is required" });
                }

                var query = _context.Employees.Where(e => e.BadgeNumber == badgeNumber.Trim());
                
                // Exclude current employee if editing
                if (!string.IsNullOrEmpty(excludeEmployeeId))
                {
                    query = query.Where(e => e.Id != excludeEmployeeId);
                }

                var existingEmployee = await query.FirstOrDefaultAsync();

                return Json(new { 
                    isAvailable = existingEmployee == null, 
                    message = existingEmployee == null ? "Badge number is available" : "Badge number already exists" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking badge number {BadgeNumber}", badgeNumber);
                return Json(new { isAvailable = false, message = "Error checking badge number" });
            }
        }

        // POST: Archive Employee (Soft Delete)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ArchiveEmployee(string employeeId)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee == null)
                {
                    return Json(new { success = false, message = "Employee not found." });
                }

                // Soft delete - mark as inactive
                employee.IsActive = false;
                employee.ModifiedAt = DateTime.UtcNow;
                employee.ModifiedBy = User.Identity.Name;
                
                _context.Employees.Update(employee);

                // Also deactivate the associated user account if exists
                if (!string.IsNullOrEmpty(employee.ApplicationUserId))
                {
                    var user = await _userManager.FindByIdAsync(employee.ApplicationUserId);
                    if (user != null)
                    {
                        // Lock the user account
                        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Employee {EmployeeName} (ID: {EmployeeId}) archived by {User}", 
                    employee.FullName, employeeId, User.Identity.Name);

                return Json(new { 
                    success = true, 
                    message = $"Employee {employee.FullName} has been archived successfully. They can be restored from the Archived Users section." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving employee {EmployeeId}", employeeId);
                return Json(new { success = false, message = "An error occurred while archiving the employee." });
            }
        }
    }
}
