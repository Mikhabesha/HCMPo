using HCMPo.Data;
using HCMPo.Models;
using HCMPo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HCMPo.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserManagementController> _logger;
        private readonly IUserEmployeeLinkService _linkService;

        public UserManagementController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserManagementController> logger,
            IUserEmployeeLinkService linkService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _linkService = linkService;
        }

        // GET: User Management Dashboard
        public async Task<IActionResult> Index(string searchTerm = null, string roleFilter = null, string statusFilter = null)
        {
            var usersQuery = _userManager.Users.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                usersQuery = usersQuery.Where(u =>
                    u.Email.Contains(searchTerm) ||
                    u.UserName.Contains(searchTerm));
            }
            var users = await usersQuery.ToListAsync();
            var usersWithRoles = new List<object>();
            int linkedCount = 0;
            int unlinkedCount = 0;
            foreach (var user in users)
            {
                var userEntity = await _userManager.FindByIdAsync(user.Id);
                var roles = await _userManager.GetRolesAsync(userEntity);
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == userEntity.EmployeeId);
                bool include = true;
                if (!string.IsNullOrEmpty(roleFilter) && (roles == null || !roles.Contains(roleFilter)))
                {
                    include = false;
                }
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    bool isLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;
                    if (statusFilter == "Active" && (isLocked || employee == null || employee.Status != EmploymentStatus.Active))
                        include = false;
                    if (statusFilter == "OnLeave" && (employee == null || employee.Status != EmploymentStatus.OnLeave))
                        include = false;
                    if (statusFilter == "Suspended" && (employee == null || employee.Status != EmploymentStatus.Suspended))
                        include = false;
                    if (statusFilter == "Terminated" && (employee == null || employee.Status != EmploymentStatus.Terminated))
                        include = false;
                    if (statusFilter == "Locked" && !isLocked)
                        include = false;
                }
                if (!include) continue;
                if (employee != null)
                    linkedCount++;
                else
                    unlinkedCount++;
                usersWithRoles.Add(new
                {
                    user.Id,
                    user.Email,
                    user.UserName,
                    user.EmailConfirmed,
                    IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                    Roles = roles,
                    Employee = employee != null ? new { Id = employee.Id, FullName = employee.FullName, BadgeNumber = employee.BadgeNumber, Status = employee.Status, OrganizationUnit = employee.OrganizationUnit?.Name ?? "N/A" } : null
                });
            }
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            int totalUsers = users.Count;
            int totalRoles = allRoles.Count;
            ViewBag.Users = usersWithRoles;
            ViewBag.AllRoles = allRoles;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.LinkedUsers = linkedCount;
            ViewBag.UnlinkedEmployees = unlinkedCount;
            ViewBag.TotalRoles = totalRoles;
            ViewBag.SelectedStatus = statusFilter;
            ViewBag.SelectedSearchTerm = searchTerm;
            ViewBag.SelectedRole = roleFilter;
            ViewBag.AllEmployees = (await _context.Employees.Where(e => e.IsActive).ToListAsync()).OrderBy(e => e.FullName).ToList();
            return View(users);
        }

        // TEMP: Fix Employee-User Links (run once, then remove)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FixEmployeeUserLinks()
        {
            var employees = await _context.Employees.ToListAsync();
            var users = await _context.Users.ToListAsync();
            int fixedLinks = 0, createdUsers = 0;

            foreach (var employee in employees)
            {
                var user = users.FirstOrDefault(u => u.EmployeeId == employee.Id);
                if (user == null)
                {
                    // No user exists for this employee, create one
                    var firstName = string.IsNullOrEmpty(employee.FirstName) ? "user" : employee.FirstName;
                    var lastName = string.IsNullOrEmpty(employee.LastName) ? "user" : employee.LastName;
                    var email = string.IsNullOrEmpty(employee.Email) ? $"{firstName.ToLower()}.{lastName.ToLower()}@hcm.com" : employee.Email;
                    var password = $"{(firstName.Length > 0 ? firstName.Substring(0, 1).ToUpper() : "U")}{(firstName.Length > 1 ? firstName.Substring(1).ToLower() : "ser")}{employee.BadgeNumber}{new Random().Next(100, 999)}!";
                    var newUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                        EmailConfirmed = true,
                        EmployeeId = employee.Id,
                        Theme = "Default",
                        Language = "en",
                        UseEthiopianCalendar = true
                    };
                    var result = await _userManager.CreateAsync(newUser, password);
                    if (result.Succeeded)
                    {
                        employee.ApplicationUserId = newUser.Id;
                            _context.Employees.Update(employee);
                        createdUsers++;
                        }
                    }
                    else
                    {
                    // User exists, ensure employee is linked back
                    if (employee.ApplicationUserId != user.Id)
                        {
                            employee.ApplicationUserId = user.Id;
                            _context.Employees.Update(employee);
                        fixedLinks++;
                    }
                }
            }
                    await _context.SaveChangesAsync();
            return Json(new { fixedLinks, createdUsers, message = "Employee-user links fixed." });
        }

        // GET: Get Employees Without User Accounts
        [HttpGet]
        public async Task<IActionResult> GetEmployeesWithoutUsers()
        {
            try
            {
                var userEmployeeIds = _context.Users
                    .Where(u => u.EmployeeId != null)
                    .Select(u => u.EmployeeId)
                    .ToHashSet();
                var employeesWithoutUsers = (await _context.Employees
                    .Where(e => e.IsActive)
                    .Include(e => e.OrganizationUnit)
                    .ToListAsync())
                    .Where(e => !userEmployeeIds.Contains(e.Id))
                    .Select(e => new
                    {
                        id = e.Id,
                        fullName = e.FullName,
                        badgeNumber = e.BadgeNumber,
                        organizationUnit = e.OrganizationUnit?.Name ?? "N/A"
                    })
                    .ToList();
                return Json(employeesWithoutUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employees without users");
                return Json(new { error = "Failed to get employees without users" });
            }
        }

        // POST: Create User Accounts for Employees Without Users
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccountsForEmployeesWithoutUsers()
        {
            try
            {
                _logger.LogInformation("Starting bulk user account creation process");
                var userEmployeeIds = _context.Users
                    .Where(u => u.EmployeeId != null)
                    .Select(u => u.EmployeeId)
                    .ToHashSet();
                var employeesWithoutUsers = (await _context.Employees
                    .Where(e => e.IsActive)
                    .Include(e => e.OrganizationUnit)
                    .Include(e => e.JobTitle)
                    .ToListAsync())
                    .Where(e => !userEmployeeIds.Contains(e.Id))
                    .ToList();
                _logger.LogInformation("Found {Count} employees without user accounts", employeesWithoutUsers.Count);
                var results = new List<object>();
                var created = 0;
                var errors = 0;
                if (!employeesWithoutUsers.Any())
                {
                    return Json(new { 
                        success = true, 
                        created = 0, 
                        errors = 0, 
                        message = "No employees found without user accounts.",
                        hasPasswordReport = false
                    });
                }
                foreach (var employee in employeesWithoutUsers)
                {
                    try
                    {
                        var firstName = string.IsNullOrEmpty(employee.FirstName) ? "user" : employee.FirstName;
                        var lastName = string.IsNullOrEmpty(employee.LastName) ? "user" : employee.LastName;
                        var email = string.IsNullOrEmpty(employee.Email) ? $"{firstName.ToLower()}.{lastName.ToLower()}@hcm.com" : employee.Email;
                        var password = $"{(firstName.Length > 0 ? firstName.Substring(0, 1).ToUpper() : "U")}{(firstName.Length > 1 ? firstName.Substring(1).ToLower() : "ser")}{employee.BadgeNumber}{new Random().Next(100, 999)}!";
                            var user = new ApplicationUser
                            {
                                UserName = email,
                                Email = email,
                                EmailConfirmed = true,
                                EmployeeId = employee.Id,
                                Theme = "Default",
                                Language = "en",
                                UseEthiopianCalendar = true
                            };
                            var createResult = await _userManager.CreateAsync(user, password);
                            if (createResult.Succeeded)
                            {
                                var defaultRole = DetermineDefaultRole(employee);
                                await _userManager.AddToRoleAsync(user, defaultRole);
                                employee.ApplicationUserId = user.Id;
                            employee.UserId = user.Id;
                                _context.Employees.Update(employee);
                                created++;
                                results.Add(new { 
                                    BadgeNumber = employee.BadgeNumber,
                                    FullName = employee.FullName,
                                    Email = email,
                                    Password = password,
                                Department = employee.OrganizationUnit?.Name ?? "N/A",
                                    JobTitle = employee.JobTitle?.Title ?? "N/A",
                                    AssignedRole = defaultRole
                                });
                            _logger.LogInformation("Created new user {Email} for employee {EmployeeName}", 
                                    email, employee.FullName);
                            }
                            else
                            {
                                errors++;
                                _logger.LogError("Failed to create user for employee {EmployeeName}: {Errors}", 
                                    employee.FullName, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                        }
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        _logger.LogError(ex, "Error creating user for employee {EmployeeName}", employee.FullName);
                    }
                }
                await _context.SaveChangesAsync();
                if (results.Any())
                {
                    TempData["PasswordReport"] = System.Text.Json.JsonSerializer.Serialize(results);
                }
                return Json(new { 
                    success = true, 
                    created,
                    errors,
                    hasPasswordReport = results.Any()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateAccountsForEmployeesWithoutUsers");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Create New User (Individual)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string email, string password, string[] roles, string? employeeId = null)
        {
            try
            {
                _logger.LogInformation("Creating individual user: {Email}, Roles: {Roles}, EmployeeId: {EmployeeId}", 
                    email, roles != null ? string.Join(",", roles) : "None", employeeId ?? "None");
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Invalid input for user creation: Email={Email}, Password={HasPassword}", 
                        email, !string.IsNullOrEmpty(password));
                    TempData["ErrorMessage"] = "Email and password are required.";
                    return RedirectToAction(nameof(Index));
                }
                var passwordErrors = new List<string>();
                if (password.Length < 6)
                    passwordErrors.Add("Password must be at least 6 characters long");
                if (!password.Any(char.IsUpper))
                    passwordErrors.Add("Password must contain at least one uppercase letter (A-Z)");
                if (!password.Any(char.IsLower))
                    passwordErrors.Add("Password must contain at least one lowercase letter (a-z)");
                if (!password.Any(char.IsDigit))
                {
                    TempData["ErrorMessage"] = "Password must contain at least one number (0-9).";
                    return RedirectToAction(nameof(Index));
                }
                if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                {
                    TempData["ErrorMessage"] = "Password must contain at least one special character (!@#$%^&*).";
                    return RedirectToAction(nameof(Index));
                }
                if (passwordErrors.Any())
                {
                    TempData["ErrorMessage"] = "Password requirements not met: " + string.Join(", ", passwordErrors);
                    return RedirectToAction(nameof(Index));
                }
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                if (!string.IsNullOrEmpty(employeeId))
                {
                    // Check if a user already exists for this employee
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
                    if (existingUser != null)
                    {
                        TempData["ErrorMessage"] = "A user already exists for this employee.";
                        return RedirectToAction(nameof(Index));
                    }
                    var employee = await _context.Employees.FindAsync(employeeId);
                    if (employee != null)
                    {
                        user.EmployeeId = employeeId;
                        if (employee.Email != email)
                        {
                            employee.Email = email;
                            _context.Employees.Update(employee);
                        }
                        _logger.LogInformation("Linking user {Email} to employee {EmployeeName} ({EmployeeId})", 
                            email, employee.FullName, employeeId);
                    }
                    else
                    {
                        _logger.LogWarning("Employee {EmployeeId} not found for user linking", employeeId);
                    }
                }
                _logger.LogInformation("Attempting to create user {Email}", email);
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} created successfully", email);
                    if (!string.IsNullOrEmpty(employeeId))
                    {
                        var employee = await _context.Employees.FindAsync(employeeId);
                        if (employee != null)
                        {
                            employee.ApplicationUserId = user.Id;
                            employee.UserId = user.Id;
                            _context.Employees.Update(employee);
                            _logger.LogInformation("Updated employee {EmployeeName} with bidirectional link to user {Email}", 
                                employee.FullName, email);
                        }
                    }
                    if (roles != null && roles.Length > 0)
                    {
                        _logger.LogInformation("Adding roles {Roles} to user {Email}", string.Join(",", roles), email);
                        await _userManager.AddToRolesAsync(user, roles);
                    }
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("User creation completed for {Email}", email);
                    TempData["SuccessMessage"] = $"User {email} created successfully!" + 
                        (!string.IsNullOrEmpty(employeeId) ? " User linked to employee." : "") +
                        (roles != null && roles.Length > 0 ? $" Assigned roles: {string.Join(", ", roles)}." : "");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create user {Email}: {Errors}", email, errors);
                    TempData["ErrorMessage"] = $"Failed to create user: {errors}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Email}", email);
                TempData["ErrorMessage"] = "An error occurred while creating the user. Please check the logs.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Download Password Report
        [HttpGet]
        public IActionResult DownloadPasswordReport()
        {
            try
            {
                var reportJson = TempData["PasswordReport"] as string;
                if (string.IsNullOrEmpty(reportJson))
                {
                    return NotFound("No password report available");
                }

                var users = System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(reportJson);
                
                // Create CSV content
                var csv = "Badge Number,Full Name,Email,Default Password,Department,Job Title,Assigned Role\n";
                foreach (var user in users!)
                {
                    var userObj = (System.Text.Json.JsonElement)user;
                    csv += $"\"{userObj.GetProperty("BadgeNumber").GetString()}\"," +
                           $"\"{userObj.GetProperty("FullName").GetString()}\"," +
                           $"\"{userObj.GetProperty("Email").GetString()}\"," +
                           $"\"{userObj.GetProperty("Password").GetString()}\"," +
                           $"\"{userObj.GetProperty("Department").GetString()}\"," +
                           $"\"{userObj.GetProperty("JobTitle").GetString()}\"," +
                           $"\"{userObj.GetProperty("AssignedRole").GetString()}\"\n";
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                var fileName = $"EmployeePasswords_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest("Error generating report");
            }
        }

        // POST: Update User Roles
        [HttpPost]
        public async Task<IActionResult> UpdateUserRoles(string userId, string[] roles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Remove all existing roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Add new roles
            if (roles != null && roles.Length > 0)
            {
                await _userManager.AddToRolesAsync(user, roles);
            }

            return Json(new { success = true, message = "User roles updated successfully" });
        }

        // POST: Lock/Unlock User
        [HttpPost]
        public async Task<IActionResult> ToggleUserLock(string userId, bool lockUser)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            if (lockUser)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }

            return Json(new { success = true, message = $"User {(lockUser ? "locked" : "unlocked")} successfully" });
        }

        // GET: Create Role
        [HttpGet]
        public async Task<IActionResult> CreateRole(string roleName, string description)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return Json(new { success = false, message = "Role name is required" });
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
            {
                return Json(new { success = false, message = "Role already exists" });
            }

            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return Json(new { success = true, message = $"Role '{roleName}' created successfully" });
            }

            return Json(new { success = false, message = "Failed to create role" });
        }

        // GET: System Info (for admin)
        [HttpGet]
        public async Task<IActionResult> SystemInfo()
        {
            var userCount = await _userManager.Users.CountAsync();
            var employeeCount = await _context.Employees.CountAsync();
            var roleCount = await _roleManager.Roles.CountAsync();
            var linkedUsers = await _userManager.Users.Where(u => u.EmployeeId != null).CountAsync();

            return Json(new
            {
                userCount,
                employeeCount,
                roleCount,
                linkedUsers,
                unlinkedEmployees = employeeCount - linkedUsers,
                systemHealth = new
                {
                    userEmployeeLinkage = linkedUsers > 0 ? "Good" : "Needs Attention",
                    rolesConfigured = roleCount >= 3 ? "Good" : "Basic"
                }
            });
        }

        // POST: Create Test Employees
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTestEmployees()
        {
            try
            {
                _logger.LogInformation("Creating test employees");
                
                // First ensure we have at least one department and job title
                var defaultOrganizationUnit = await _context.OrganizationUnits.FirstOrDefaultAsync(ou => ou.Type == OrganizationUnitType.Department);
                var defaultJobTitle = await _context.JobTitles.FirstOrDefaultAsync();
                
                if (defaultOrganizationUnit == null)
                {
                    // Create a default department
                    defaultOrganizationUnit = new OrganizationUnit
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "General",
                        Description = "General Department",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "system"
                    };
                    _context.OrganizationUnits.Add(defaultOrganizationUnit);
                    await _context.SaveChangesAsync();
                }
                
                if (defaultJobTitle == null)
                {
                    // Create a default job title
                    defaultJobTitle = new JobTitle
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = "Employee",
                        Description = "General Employee",
                        Grade = "E1",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "system"
                    };
                    _context.JobTitles.Add(defaultJobTitle);
                    await _context.SaveChangesAsync();
                }

                var testEmployees = new List<Employee>
                {
                    new Employee
                    {
                        Id = Guid.NewGuid().ToString(),
                        FirstName = "John",
                        LastName = "Doe",
                        BadgeNumber = "TEST001",
                        Email = "john@hcm.com",
                        PhoneNumber = "+251911123456",
                        HireDate = DateTime.Now.AddYears(-2),
                        EmploymentDate = DateTime.Now.AddYears(-2),
                        DateOfBirth = DateTime.Now.AddYears(-30),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        OrganizationUnitId = defaultOrganizationUnit.Id,
                        JobTitleId = defaultJobTitle.Id,
                        BasicSalary = 15000,
                        Salary = 15000,
                        Gender = "Male",
                        Status = EmploymentStatus.Active,
                        CreatedBy = "system"
                    },
                    new Employee
                    {
                        Id = Guid.NewGuid().ToString(),
                        FirstName = "Jane",
                        LastName = "Smith",
                        BadgeNumber = "TEST002",
                        Email = "jane@hcm.com",
                        PhoneNumber = "+251911123457",
                        HireDate = DateTime.Now.AddYears(-1),
                        EmploymentDate = DateTime.Now.AddYears(-1),
                        DateOfBirth = DateTime.Now.AddYears(-28),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        OrganizationUnitId = defaultOrganizationUnit.Id,
                        JobTitleId = defaultJobTitle.Id,
                        BasicSalary = 16000,
                        Salary = 16000,
                        Gender = "Female",
                        Status = EmploymentStatus.Active,
                        CreatedBy = "system"
                    },
                    new Employee
                    {
                        Id = Guid.NewGuid().ToString(),
                        FirstName = "Mike",
                        LastName = "Johnson",
                        BadgeNumber = "TEST003",
                        Email = "mike@hcm.com",
                        PhoneNumber = "+251911123458",
                        HireDate = DateTime.Now.AddMonths(-6),
                        EmploymentDate = DateTime.Now.AddMonths(-6),
                        DateOfBirth = DateTime.Now.AddYears(-35),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        OrganizationUnitId = defaultOrganizationUnit.Id,
                        JobTitleId = defaultJobTitle.Id,
                        BasicSalary = 14000,
                        Salary = 14000,
                        Gender = "Male",
                        Status = EmploymentStatus.Active,
                        CreatedBy = "system"
                    }
                };

                // Check if test employees already exist
                var existingBadges = await _context.Employees
                    .Where(e => testEmployees.Select(te => te.BadgeNumber).Contains(e.BadgeNumber))
                    .Select(e => e.BadgeNumber)
                    .ToListAsync();

                var newEmployees = testEmployees.Where(te => !existingBadges.Contains(te.BadgeNumber)).ToList();

                if (newEmployees.Any())
                {
                    _context.Employees.AddRange(newEmployees);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Created {Count} test employees", newEmployees.Count);
                    return Json(new { 
                        success = true, 
                        message = $"Created {newEmployees.Count} test employees successfully!" 
                    });
                }
                else
                {
                    return Json(new { 
                        success = true, 
                        message = "Test employees already exist in the system." 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test employees");
                return Json(new { success = false, message = "Failed to create test employees: " + ex.Message });
            }
        }

        // POST: Reset User Password
        [HttpPost]
        public async Task<IActionResult> ResetUserPassword(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Generate a secure temporary password
                var tempPassword = $"Temp{DateTime.Now.ToString("MMdd")}!{new Random().Next(100, 999)}";
                
                // Reset password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, tempPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Password reset for user {Email}", user.Email);
                    return Json(new { 
                        success = true, 
                        message = "Password reset successfully", 
                        tempPassword = tempPassword 
                    });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, message = $"Failed to reset password: {errors}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", userId);
                return Json(new { success = false, message = "An error occurred while resetting password" });
            }
        }

        // POST: Link User to Employee
        [HttpPost]
        public async Task<IActionResult> LinkUserToEmployee(string userId, string employeeId)
        {
            var (success, message) = await _linkService.LinkUserToEmployeeAsync(userId, employeeId);
            return Json(new { success, message });
        }

        // POST: Deactivate (Archive) User
        [HttpPost]
        public async Task<IActionResult> DeactivateUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }
                // Lock the user account
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                // Deactivate the associated employee (if any)
                if (!string.IsNullOrEmpty(user.EmployeeId))
                {
                    var employee = await _context.Employees.FindAsync(user.EmployeeId);
                    if (employee != null)
                    {
                        employee.IsActive = false;
                        _context.Employees.Update(employee);
                    }
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "User deactivated and archived successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", userId);
                return Json(new { success = false, message = "An error occurred while deactivating the user." });
            }
        }

        // GET: Archived Users
        [HttpGet]
        public async Task<IActionResult> ArchivedUsers()
        {
            // Find users whose associated employee is inactive
            var archivedUsers = await _userManager.Users
                .Where(u => u.EmployeeId != null)
                .Join(_context.Employees.Where(e => !e.IsActive).Include(e => e.OrganizationUnit),
                      u => u.EmployeeId,
                      e => e.Id,
                      (u, e) => new { User = u, Employee = e })
                .ToListAsync();

            var usersWithRoles = new List<object>();
            foreach (var pair in archivedUsers)
            {
                var roles = await _userManager.GetRolesAsync(pair.User);
                usersWithRoles.Add(new
                {
                    pair.User.Id,
                    pair.User.Email,
                    pair.User.UserName,
                    pair.User.EmailConfirmed,
                    IsLocked = pair.User.LockoutEnd.HasValue && pair.User.LockoutEnd > DateTimeOffset.UtcNow,
                    Roles = roles,
                    Employee = new { 
                        pair.Employee.Id, 
                        pair.Employee.FullName, 
                        pair.Employee.BadgeNumber,
                        OrganizationUnit = pair.Employee.OrganizationUnit?.Name ?? "N/A"
                    }
                });
            }
            ViewBag.ArchivedUsers = usersWithRoles;
            return View();
        }

        // POST: Restore User
        [HttpPost]
        public async Task<IActionResult> RestoreUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }
                // Unlock the user account
                await _userManager.SetLockoutEndDateAsync(user, null);
                // Reactivate the associated employee (if any)
                if (!string.IsNullOrEmpty(user.EmployeeId))
                {
                    var employee = await _context.Employees.FindAsync(user.EmployeeId);
                    if (employee != null)
                    {
                        employee.IsActive = true;
                        _context.Employees.Update(employee);
                    }
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "User restored and reactivated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring user {UserId}", userId);
                return Json(new { success = false, message = "An error occurred while restoring the user." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkRestoreUsers([FromBody] List<string> userIds)
        {
            int restored = 0, errors = 0;
            foreach (var userId in userIds)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null) { errors++; continue; }
                    await _userManager.SetLockoutEndDateAsync(user, null);
                    if (!string.IsNullOrEmpty(user.EmployeeId))
                    {
                        var employee = await _context.Employees.FindAsync(user.EmployeeId);
                        if (employee != null)
                        {
                            employee.IsActive = true;
                    _context.Employees.Update(employee);
                        }
                    }
                    restored++;
                }
                catch
                {
                    errors++;
                }
            }
                    await _context.SaveChangesAsync();
            return Json(new { success = true, restored, errors });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MakeAdminTeamLeaderAndDirectorForAll(string employeeId = null)
        {
            Employee adminEmployee = null;
            ApplicationUser adminUser = null;
            if (!string.IsNullOrEmpty(employeeId))
            {
                adminEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
                if (adminEmployee != null && !string.IsNullOrEmpty(adminEmployee.ApplicationUserId))
                {
                    adminUser = await _userManager.FindByIdAsync(adminEmployee.ApplicationUserId);
                }
            }
            if (adminUser == null || adminEmployee == null)
            {
                // Fallback to default admin
                adminUser = await _userManager.FindByEmailAsync("admin@hcmpo.com");
                if (adminUser == null)
                    return Json(new { success = false, message = "Admin user not found." });
                adminEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.ApplicationUserId == adminUser.Id);
                if (adminEmployee == null)
                    return Json(new { success = false, message = "Admin employee profile not found." });
            }
            // Assign TeamLeader and Director roles if not already present
            var rolesToAdd = new List<string>();
            if (!await _userManager.IsInRoleAsync(adminUser, "TeamLeader"))
                rolesToAdd.Add("TeamLeader");
            if (!await _userManager.IsInRoleAsync(adminUser, "Director"))
                rolesToAdd.Add("Director");
            if (rolesToAdd.Any())
                await _userManager.AddToRolesAsync(adminUser, rolesToAdd);
            // Set as TeamLeader and Director for all departments
            var organizationUnits = await _context.OrganizationUnits.Where(ou => ou.Type == OrganizationUnitType.Department).ToListAsync();
            foreach (var dept in organizationUnits)
            {
                dept.TeamLeaderId = adminEmployee.Id;
                dept.DirectorId = adminEmployee.Id;
                _context.OrganizationUnits.Update(dept);
            }
            // Set as TeamLeader and Director for all employees
            var employees = await _context.Employees.ToListAsync();
            foreach (var emp in employees)
            {
                emp.TeamLeaderId = adminEmployee.Id;
                emp.DirectorId = adminEmployee.Id;
                _context.Employees.Update(emp);
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"{adminEmployee.FullName} is now Team Leader and Director for all departments and employees." });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignAdminAsApproverForAllPendingLeaveRequests(string employeeId = null)
        {
            Employee adminEmployee = null;
            ApplicationUser adminUser = null;
            if (!string.IsNullOrEmpty(employeeId))
            {
                adminEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
                if (adminEmployee != null && !string.IsNullOrEmpty(adminEmployee.ApplicationUserId))
                {
                    adminUser = await _userManager.FindByIdAsync(adminEmployee.ApplicationUserId);
                }
            }
            if (adminUser == null || adminEmployee == null)
            {
                // Fallback to default admin
                adminUser = await _userManager.FindByEmailAsync("admin@hcmpo.com");
                if (adminUser == null)
                    return Json(new { success = false, message = "Admin user not found." });
                adminEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.ApplicationUserId == adminUser.Id);
                if (adminEmployee == null)
                    return Json(new { success = false, message = "Admin employee profile not found." });
            }
            // Get all leave requests that are not fully approved or rejected
            var pendingRequests = await _context.LeaveRequests
                .Where(r => r.Status != HCMPo.Models.Enums.LeaveRequestStatus.HRApproved)
                .ToListAsync();
            pendingRequests = pendingRequests
                .Where(r => !r.Status.ToString().Contains("Rejected"))
                .ToList();
            foreach (var request in pendingRequests)
            {
                request.TeamLeaderId = adminEmployee.Id;
                request.DirectorId = adminEmployee.Id;
                request.HRId = adminEmployee.Id;
                request.CurrentApprover = adminEmployee.Id;
                _context.LeaveRequests.Update(request);
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"{adminEmployee.FullName} is now the approver for all pending leave requests (for testing only)." });
        }

        [HttpPost]
        public async Task<IActionResult> UnlinkUserFromEmployee(string userId, string employeeId)
        {
            var (success, message) = await _linkService.UnlinkUserFromEmployeeAsync(userId, employeeId);
            return Json(new { success, message });
        }

        private string DetermineDefaultRole(Employee employee)
        {
            // Default role logic based on job title or department
            if (employee.JobTitle?.Title?.ToLower().Contains("manager") == true ||
                employee.JobTitle?.Title?.ToLower().Contains("director") == true)
            {
                return "Manager";
            }
            else if (employee.JobTitle?.Title?.ToLower().Contains("hr") == true)
            {
                return "HR";
            }
            else if (employee.JobTitle?.Title?.ToLower().Contains("admin") == true)
            {
                return "Admin";
            }
            return "Employee";
        }
    }
} 
