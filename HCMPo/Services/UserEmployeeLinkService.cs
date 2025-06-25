using HCMPo.Data;
using HCMPo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HCMPo.Services
{
    public interface IUserEmployeeLinkService
    {
        Task<(bool success, string message)> LinkUserToEmployeeAsync(string userId, string employeeId);
        Task<(bool success, string message)> UnlinkUserFromEmployeeAsync(string userId, string employeeId);
    }

    public class UserEmployeeLinkService : IUserEmployeeLinkService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserEmployeeLinkService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<(bool success, string message)> LinkUserToEmployeeAsync(string userId, string employeeId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, "User not found");

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return (false, "Employee not found");

            // Prevent duplicate links
            var existingLink = await _userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
            if (existingLink != null && existingLink.Id != userId)
                return (false, "Employee is already linked to another user");

            user.EmployeeId = employeeId;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                employee.ApplicationUserId = user.Id;
                employee.UserId = user.Id; // For backward compatibility
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();
                return (true, $"User linked to {employee.FullName} successfully");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, $"Failed to link user: {errors}");
            }
        }

        public async Task<(bool success, string message)> UnlinkUserFromEmployeeAsync(string userId, string employeeId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, "User not found");

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return (false, "Employee not found");

            // Only unlink if the link matches
            if (user.EmployeeId == employeeId)
            {
                user.EmployeeId = null;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return (false, $"Failed to unlink user: {errors}");
                }
            }

            if (employee.ApplicationUserId == userId)
            {
                employee.ApplicationUserId = null;
                employee.UserId = null;
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();
            }

            return (true, $"User and employee unlinked successfully");
        }
    }
} 