using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HCMPo.Models
{
    // Non-static class for logging purposes
    public class DbInitializerLogger { }

    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            context.Database.EnsureCreated();

            // Check if there are any users
            if (!context.Users.Any())
            {
                // Create roles
                var roles = new[] { "Admin", "HR", "Employee" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Create admin user
                var adminUser = new IdentityUser
                {
                    UserName = "admin@hcmpo.com",
                    Email = "admin@hcmpo.com",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Check if there are any departments
            if (!context.Departments.Any())
            {
                var departments = new[]
                {
                    new HCMPo.Models.Department { Id = Guid.NewGuid().ToString(), Name = "Human Resources", Description = "HR Department" },
                    new HCMPo.Models.Department { Id = Guid.NewGuid().ToString(), Name = "Information Technology", Description = "IT Department" },
                    new HCMPo.Models.Department { Id = Guid.NewGuid().ToString(), Name = "Finance", Description = "Finance Department" }
                };

                context.Departments.AddRange(departments);
                await context.SaveChangesAsync();
            }

            // Check if there are any job titles
            if (!context.JobTitles.Any())
            {
                var jobTitles = new[]
                {
                    new HCMPo.Models.JobTitle { Id = Guid.NewGuid().ToString(), Title = "HR Manager", Description = "Human Resources Manager" },
                    new HCMPo.Models.JobTitle { Id = Guid.NewGuid().ToString(), Title = "Software Developer", Description = "Software Developer" },
                    new HCMPo.Models.JobTitle { Id = Guid.NewGuid().ToString(), Title = "Accountant", Description = "Finance Accountant" }
                };

                context.JobTitles.AddRange(jobTitles);
                await context.SaveChangesAsync();
            }

            // Check if there are any leave types
            if (!context.LeaveTypes.Any())
            {
                var leaveTypes = new[]
                {
                    new HCMPo.Models.LeaveType 
                    { 
                        Id = Guid.NewGuid().ToString(), 
                        Name = "Annual Leave", 
                        DefaultDays = 20,
                        Description = "Paid vacation leave",
                        IsPaidLeave = true,
                        RequiresAttachment = false,
                        RequiresApproval = true,
                        MaxDaysPerRequest = 30,
                        AllowHalfDay = true
                    },
                    new HCMPo.Models.LeaveType 
                    { 
                        Id = Guid.NewGuid().ToString(), 
                        Name = "Sick Leave", 
                        DefaultDays = 10,
                        Description = "Paid sick leave",
                        IsPaidLeave = true,
                        RequiresAttachment = true,
                        RequiresApproval = true,
                        MaxDaysPerRequest = 5,
                        AllowHalfDay = true
                    },
                    new HCMPo.Models.LeaveType 
                    { 
                        Id = Guid.NewGuid().ToString(), 
                        Name = "Maternity Leave", 
                        DefaultDays = 90,
                        Description = "Maternity leave",
                        IsPaidLeave = true,
                        RequiresAttachment = true,
                        RequiresApproval = true,
                        MaxDaysPerRequest = 90,
                        AllowHalfDay = false
                    },
                    new HCMPo.Models.LeaveType 
                    { 
                        Id = Guid.NewGuid().ToString(), 
                        Name = "Paternity Leave", 
                        DefaultDays = 5,
                        Description = "Paternity leave",
                        IsPaidLeave = true,
                        RequiresAttachment = false,
                        RequiresApproval = true,
                        MaxDaysPerRequest = 5,
                        AllowHalfDay = false
                    }
                };

                context.LeaveTypes.AddRange(leaveTypes);
                await context.SaveChangesAsync();
            }
        }
    }
} 