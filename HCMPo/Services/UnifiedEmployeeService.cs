using HCMPo.Data;
using HCMPo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace HCMPo.Services
{
    public interface IUnifiedEmployeeService
    {
        Task<Employee> CreateEmployeeAsync(Employee employee);
        Task<bool> SyncEmployeeToAttDbAsync(Employee employee);
        Task<ApplicationUser> CreateUserAccountAsync(Employee employee, string password, string[] roles);
        Task<bool> LinkEmployeeToUserAsync(string employeeId, string userId);
        Task<List<Employee>> GetUnifiedEmployeesAsync();
        Task<Employee> GetUnifiedEmployeeAsync(string employeeId);
    }

    public class UnifiedEmployeeService : IUnifiedEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UnifiedEmployeeService> _logger;

        public UnifiedEmployeeService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ILogger<UnifiedEmployeeService> logger)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            try
            {
                // 1. Create employee in HCM system
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Employee {FullName} created in HCM system with ID {Id}", 
                    employee.FullName, employee.Id);

                // 2. Automatically sync to Att_db
                await SyncEmployeeToAttDbAsync(employee);

                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating unified employee {FullName}", employee.FullName);
                throw;
            }
        }

        public async Task<bool> SyncEmployeeToAttDbAsync(Employee employee)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("ZKTimeConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogWarning("ZKTimeConnection not configured, skipping Att_db sync");
                    return false;
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Check if employee already exists in Att_db
                    var checkQuery = "SELECT COUNT(*) FROM userinfo WHERE badgenumber = @BadgeNumber";
                    using (var checkCmd = new SqlCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@BadgeNumber", employee.BadgeNumber);
                        var exists = (int)await checkCmd.ExecuteScalarAsync() > 0;

                        if (exists)
                        {
                            // Update existing employee
                            var updateQuery = @"
                                UPDATE userinfo SET 
                                    name = @Name,
                                    defaultdeptid = 1
                                WHERE badgenumber = @BadgeNumber";

                            using (var updateCmd = new SqlCommand(updateQuery, connection))
                            {
                                updateCmd.Parameters.AddWithValue("@Name", employee.FullName);
                                updateCmd.Parameters.AddWithValue("@BadgeNumber", employee.BadgeNumber);
                                await updateCmd.ExecuteNonQueryAsync();
                            }
                        }
                        else
                        {
                            // Insert new employee
                            var insertQuery = @"
                                INSERT INTO userinfo (badgenumber, name, defaultdeptid) 
                                VALUES (@BadgeNumber, @Name, 1)";

                            using (var insertCmd = new SqlCommand(insertQuery, connection))
                            {
                                insertCmd.Parameters.AddWithValue("@BadgeNumber", employee.BadgeNumber);
                                insertCmd.Parameters.AddWithValue("@Name", employee.FullName);
                                await insertCmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }

                _logger.LogInformation("Employee {FullName} synced to Att_db successfully", employee.FullName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing employee {FullName} to Att_db", employee.FullName);
                return false;
            }
        }

        public async Task<ApplicationUser> CreateUserAccountAsync(Employee employee, string password, string[] roles)
        {
            try
            {
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

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    if (roles != null && roles.Length > 0)
                    {
                        await _userManager.AddToRolesAsync(user, roles);
                    }

                    // Set ApplicationUserId on employee and save
                    employee.ApplicationUserId = user.Id;
                    _context.Employees.Update(employee);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User account created for employee {FullName}", employee.FullName);
                    return user;
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create user account: {errors}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user account for employee {FullName}", employee.FullName);
                throw;
            }
        }

        public async Task<bool> LinkEmployeeToUserAsync(string employeeId, string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee == null) return false;

                user.EmployeeId = employeeId;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} linked to employee {FullName}", user.Email, employee.FullName);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking employee {EmployeeId} to user {UserId}", employeeId, userId);
                return false;
            }
        }

        public async Task<List<Employee>> GetUnifiedEmployeesAsync()
        {
            return await _context.Employees
                .Include(e => e.OrganizationUnit)
                .Include(e => e.JobTitle)
                .Include(e => e.ApplicationUser)
                .Where(e => e.IsActive)
                .OrderBy(e => e.BadgeNumber)
                .ToListAsync();
        }

        public async Task<Employee> GetUnifiedEmployeeAsync(string employeeId)
        {
            return await _context.Employees
                .Include(e => e.OrganizationUnit)
                .Include(e => e.JobTitle)
                .Include(e => e.ApplicationUser)
                .FirstOrDefaultAsync(e => e.Id == employeeId);
        }
    }
} 