using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HCMPo.Models
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<PerformanceReview> PerformanceReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.JobTitle)
                .WithMany()
                .HasForeignKey(e => e.JobTitleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure leave-related relationships
            modelBuilder.Entity<EmployeeLeave>()
                .HasOne(el => el.Employee)
                .WithMany()
                .HasForeignKey(el => el.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeLeave>()
                .HasOne(el => el.LeaveType)
                .WithMany()
                .HasForeignKey(el => el.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.Employee)
                .WithMany()
                .HasForeignKey(lr => lr.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.LeaveType)
                .WithMany()
                .HasForeignKey(lr => lr.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = "1", Name = "Human Resources", Description = "HR Department" },
                new Department { Id = "2", Name = "Information Technology", Description = "IT Department" },
                new Department { Id = "3", Name = "Finance", Description = "Finance Department" }
            );

            modelBuilder.Entity<JobTitle>().HasData(
                new JobTitle { Id = "1", Title = "HR Manager", Description = "Human Resources Manager" },
                new JobTitle { Id = "2", Title = "Software Developer", Description = "IT Software Developer" },
                new JobTitle { Id = "3", Title = "Accountant", Description = "Finance Accountant" }
            );

            // Seed leave types
            modelBuilder.Entity<LeaveType>().HasData(
                new LeaveType
                {
                    Id = "1",
                    Name = "Annual Leave",
                    Description = "Paid vacation leave",
                    DefaultDays = 20,
                    IsPaidLeave = true,
                    RequiresAttachment = false,
                    RequiresApproval = true,
                    MaxDaysPerRequest = 30,
                    AllowHalfDay = true
                },
                new LeaveType
                {
                    Id = "2",
                    Name = "Sick Leave",
                    Description = "Paid sick leave",
                    DefaultDays = 10,
                    IsPaidLeave = true,
                    RequiresAttachment = true,
                    RequiresApproval = true,
                    MaxDaysPerRequest = 5,
                    AllowHalfDay = true
                },
                new LeaveType
                {
                    Id = "3",
                    Name = "Unpaid Leave",
                    Description = "Unpaid leave of absence",
                    DefaultDays = 0,
                    IsPaidLeave = false,
                    RequiresAttachment = false,
                    RequiresApproval = true,
                    MaxDaysPerRequest = 30,
                    AllowHalfDay = false
                }
            );
        }
    }
}