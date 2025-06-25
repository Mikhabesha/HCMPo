using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HCMPo.Data;
using HCMPo.Models;
using Microsoft.Data.SqlClient;

namespace HCMPo.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeeSetupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmployeeSetupController> _logger;

        // The 14 Directorates for your organization
        private readonly Dictionary<string, DirectorateInfo> _directorates = new()
        {
            { "01", new DirectorateInfo { Name = "Office of the Director General", Code = "OCG" } },
            { "02", new DirectorateInfo { Name = "Operations Management Directorate", Code = "RMD" } },
            { "03", new DirectorateInfo { Name = "Member Services Directorate", Code = "COD" } },
            { "04", new DirectorateInfo { Name = "Benefits Administration Directorate", Code = "TAD" } },
            { "05", new DirectorateInfo { Name = "Investigation and Intelligence Directorate", Code = "IID" } },
            { "06", new DirectorateInfo { Name = "Legal Affairs Directorate", Code = "LAD" } },
            { "07", new DirectorateInfo { Name = "Internal Audit Directorate", Code = "IAD" } },
            { "08", new DirectorateInfo { Name = "Human Resources Directorate", Code = "HRD" } },
            { "09", new DirectorateInfo { Name = "Finance and Procurement Directorate", Code = "FPD" } },
            { "10", new DirectorateInfo { Name = "Information Technology Directorate", Code = "ITD" } },
            { "11", new DirectorateInfo { Name = "Planning and Performance Directorate", Code = "PPD" } },
            { "12", new DirectorateInfo { Name = "Corporate Communication Directorate", Code = "CCD" } },
            { "13", new DirectorateInfo { Name = "Policy and Research Directorate", Code = "RID" } },
            { "14", new DirectorateInfo { Name = "Regional Operations Directorate", Code = "ROD" } }
        };

        public EmployeeSetupController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<EmployeeSetupController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        // GET: Employee Setup Dashboard
        public async Task<IActionResult> Index()
        {
            var stats = new EmployeeSetupStats
            {
                TotalEmployees = await _context.Employees.CountAsync(),
                EmployeesWithUsers = await _context.Employees.CountAsync(e => e.ApplicationUserId != null),
                EmployeesWithoutUsers = await _context.Employees.CountAsync(e => e.ApplicationUserId == null),
                ZKTimeEmployeeCount = await GetZKTimeEmployeeCountAsync()
            };

            ViewBag.Stats = stats;
            return View();
        }

        // Create User Accounts for Existing Employees
        [HttpPost]
        public async Task<IActionResult> CreateUserAccounts()
        {
            try
            {
                var employeesWithoutUsers = await _context.Employees
                    .Where(e => e.ApplicationUserId == null)
                    .ToListAsync();

                var result = new UserCreationResult();
                var createdUsers = new List<EmployeeUserInfo>();

                foreach (var employee in employeesWithoutUsers)
                {
                    try
                    {
                        // Generate default password
                        var firstName = string.IsNullOrEmpty(employee.FirstName) ? "user" : employee.FirstName;
                        var lastName = string.IsNullOrEmpty(employee.LastName) ? "user" : employee.LastName;
                        var password = $"{(firstName.Length > 0 ? firstName.Substring(0, 1).ToUpper() : "U")}{(firstName.Length > 1 ? firstName.Substring(1).ToLower() : "ser")}{employee.BadgeNumber}{new Random().Next(100, 999)}!";
                        
                        // Create email if not exists
                        if (string.IsNullOrEmpty(employee.Email) || employee.Email == "N/A")
                        {
                            employee.Email = $"{firstName.ToLower()}@hcm.com";
                            _context.Employees.Update(employee);
                        }

                        // Check if user already exists
                        var existingUser = await _userManager.FindByEmailAsync(employee.Email);
                        if (existingUser != null)
                        {
                            // Link existing user to employee
                            existingUser.EmployeeId = employee.Id;
                            await _userManager.UpdateAsync(existingUser);
                            employee.ApplicationUserId = existingUser.Id;
                            result.LinkedCount++;
                        }
                        else
                        {
                            // Create new user
                            var user = new ApplicationUser
                            {
                                UserName = employee.Email,
                                Email = employee.Email,
                                EmailConfirmed = true,
                                EmployeeId = employee.Id,
                                Theme = "Default",
                                Language = "en",
                                UseEthiopianCalendar = true
                            };

                            var createResult = await _userManager.CreateAsync(user, password);
                            if (createResult.Succeeded)
                            {
                                // Assign default role
                                await _userManager.AddToRoleAsync(user, "Employee");
                                
                                // Link user to employee
                                employee.ApplicationUserId = user.Id;
                                
                                result.CreatedCount++;
                                createdUsers.Add(new EmployeeUserInfo
                                {
                                    BadgeNumber = employee.BadgeNumber,
                                    FullName = employee.FullName,
                                    Email = employee.Email,
                                    Password = password,
                                    Department = employee.OrganizationUnit?.Name ?? "N/A"
                                });
                            }
                            else
                            {
                                result.ErrorCount++;
                                result.Errors.Add($"{employee.FullName}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result.ErrorCount++;
                        result.Errors.Add($"{employee.FullName}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();

                // Store password report in TempData for download
                TempData["PasswordReport"] = System.Text.Json.JsonSerializer.Serialize(createdUsers);

                return Json(new { 
                    success = true, 
                    created = result.CreatedCount,
                    linked = result.LinkedCount,
                    errors = result.ErrorCount,
                    errorMessages = result.Errors.Take(5).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user accounts");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Download Password Report
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

                var users = System.Text.Json.JsonSerializer.Deserialize<List<EmployeeUserInfo>>(reportJson);
                
                // Create CSV content
                var csv = "Badge Number,Full Name,Email,Default Password,Organization Unit\n";
                foreach (var user in users!)
                {
                    csv += $"{user.BadgeNumber},{user.FullName},{user.Email},{user.Password},{user.Department}\n";
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                var fileName = $"EmployeePasswords_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating password report");
                return BadRequest("Error generating report");
            }
        }

        // Get ZKTime Employee Preview
        [HttpGet]
        public async Task<IActionResult> GetZKTimePreview()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("ZKTimeConnection");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT TOP 50
                        u.USERID,
                        u.BADGENUMBER,
                        u.NAME,
                        u.DEFAULTDEPTID,
                        d.DEPTNAME,
                        u.PRIVILEGE
                    FROM USERINFO u
                    LEFT JOIN DEPARTMENTS d ON u.DEFAULTDEPTID = d.DEPTID
                    WHERE u.NAME IS NOT NULL AND u.NAME != ''
                    ORDER BY u.DEFAULTDEPTID, u.USERID";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                var employees = new List<object>();
                while (await reader.ReadAsync())
                {
                    var deptId = reader["DEFAULTDEPTID"]?.ToString();
                    var userLevel = reader["PRIVILEGE"]?.ToString();
                    
                    employees.Add(new
                    {
                        UserId = reader["USERID"].ToString(),
                        BadgeNumber = reader["BADGENUMBER"]?.ToString() ?? reader["USERID"].ToString(),
                        Name = reader["NAME"].ToString(),
                        DepartmentId = deptId,
                        DepartmentName = reader["DEPTNAME"]?.ToString(),
                        UserLevel = userLevel,
                        ProposedDirectorate = GetDirectorateForDept(deptId),
                        ProposedRole = GetRoleForUserLevel(userLevel),
                        EmailToCreate = GenerateEmail(reader["NAME"].ToString(), reader["BADGENUMBER"]?.ToString() ?? reader["USERID"].ToString())
                    });
                }

                return Json(new { success = true, employees });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ZKTime preview");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Assign Roles Based on ZKTime User Levels
        [HttpPost]
        public async Task<IActionResult> AssignRolesByUserLevel()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("ZKTimeConnection");
                var roleAssignments = 0;

                // Get ZKTime user levels
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT u.USERID, u.PRIVILEGE FROM USERINFO u WHERE u.PRIVILEGE IS NOT NULL";
                    
                    using var command = new SqlCommand(query, connection);
                    using var reader = await command.ExecuteReaderAsync();
                    
                    var userLevels = new Dictionary<string, string>();
                    while (await reader.ReadAsync())
                    {
                        userLevels[reader["USERID"].ToString()!] = reader["PRIVILEGE"].ToString()!;
                    }

                    // Update employee roles
                    foreach (var kvp in userLevels)
                    {
                        var employee = await _context.Employees
                            .Include(e => e.ApplicationUser)
                            .FirstOrDefaultAsync(e => e.BadgeNumber == kvp.Key);

                        if (employee?.ApplicationUser != null)
                        {
                            var newRole = GetRoleForUserLevel(kvp.Value);
                            var currentRoles = await _userManager.GetRolesAsync(employee.ApplicationUser);
                            
                            if (!currentRoles.Contains(newRole))
                            {
                                await _userManager.RemoveFromRolesAsync(employee.ApplicationUser, currentRoles);
                                await _userManager.AddToRoleAsync(employee.ApplicationUser, newRole);
                                roleAssignments++;
                            }
                        }
                    }
                }

                return Json(new { success = true, assigned = roleAssignments });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Setup Department-to-Directorate Mapping
        [HttpPost]
        public async Task<IActionResult> SetupDirectorateMapping()
        {
            try
            {
                var departments = await _context.OrganizationUnits.Where(ou => ou.Type == OrganizationUnitType.Department).ToListAsync();
                var mapped = 0;

                foreach (var dept in departments)
                {
                    // Simple mapping logic - you can enhance this
                    var directorateInfo = GetDirectorateInfoForDepartment(dept.Name);
                    if (directorateInfo != null)
                    {
                        dept.Description = $"{dept.Description} - {directorateInfo.Name}";
                        mapped++;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, mapped });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper Methods
        private string GenerateDefaultPassword(string badgeNumber, string firstName)
        {
            var random = new Random();
            var randomNumber = random.Next(100, 999);
            return $"{firstName.ToLower()}{badgeNumber}{randomNumber}";
        }

        private string GenerateEmail(string fullName, string badgeNumber)
        {
            var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.Length > 0 ? nameParts[0] : fullName;
            
            return $"{firstName.ToLower()}@hcm.com";
        }

        private string GetDirectorateForDept(string? deptId)
        {
            if (string.IsNullOrEmpty(deptId) || !_directorates.ContainsKey(deptId))
                return _directorates["14"].Name; // Default to Regional Operations
            
            return _directorates[deptId].Name;
        }

        private DirectorateInfo? GetDirectorateInfoForDepartment(string? deptCode)
        {
            if (string.IsNullOrEmpty(deptCode)) return null;
            return _directorates.ContainsKey(deptCode) ? _directorates[deptCode] : null;
        }

        private string GetRoleForUserLevel(string? userLevel)
        {
            return userLevel switch
            {
                "14" => "Admin",
                "6" => "Director",
                "3" => "TeamLeader", 
                "2" => "HR",
                _ => "Employee"
            };
        }

        private async Task<int> GetZKTimeEmployeeCountAsync()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("ZKTimeConnection");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand("SELECT COUNT(*) FROM USERINFO WHERE NAME IS NOT NULL AND NAME != ''", connection);
                return (int)await command.ExecuteScalarAsync();
            }
            catch
            {
                return 0;
            }
        }
    }

    // Supporting classes
    public class DirectorateInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class EmployeeSetupStats
    {
        public int TotalEmployees { get; set; }
        public int EmployeesWithUsers { get; set; }
        public int EmployeesWithoutUsers { get; set; }
        public int ZKTimeEmployeeCount { get; set; }
    }

    public class UserCreationResult
    {
        public int CreatedCount { get; set; }
        public int LinkedCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class EmployeeUserInfo
    {
        public string BadgeNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty; // For legacy compatibility, but represents Organization Unit
    }
} 
