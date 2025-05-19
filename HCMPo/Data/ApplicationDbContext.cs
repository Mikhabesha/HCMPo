using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HCMPo.Models;

namespace HCMPo.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<PayrollDeduction> PayrollDeductions { get; set; }
        public DbSet<PayrollDeclaration> PayrollDeclarations { get; set; }
        public DbSet<EmployeeTax> EmployeeTaxes { get; set; }
        public DbSet<TaxSetting> TaxSettings { get; set; }
        public DbSet<SyncLog> SyncLogs { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<EmployeeLeave> EmployeeLeaves { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<PerformanceReview> PerformanceReviews { get; set; }
        public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
        public DbSet<EmployeeSkill> EmployeeSkills { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
        public DbSet<EmployeeDependent> EmployeeDependents { get; set; }
        public DbSet<EmployeeEducation> EmployeeEducation { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<TaxBracket> TaxBrackets { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<PayrollConfiguration> PayrollConfigurations { get; set; }
        public DbSet<DeductionType> DeductionTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure one-to-one relationship between Employee and ApplicationUser
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Employee relationships
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.JobTitle)
                .WithMany(j => j.Employees)
                .HasForeignKey(e => e.JobTitleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Employee-Supervisor relationship
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Supervisor)
                .WithMany()
                .HasForeignKey(e => e.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Employee decimal precision
            modelBuilder.Entity<Employee>()
                .Property(e => e.Salary)
                .HasPrecision(18, 2);

            // Configure EmployeeLeave relationships and decimal precision
            modelBuilder.Entity<EmployeeLeave>()
                .HasOne(el => el.Employee)
                .WithMany(e => e.Leaves)
                .HasForeignKey(el => el.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeLeave>()
                .Property(el => el.RemainingDays)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EmployeeLeave>()
                .Property(el => el.TotalDays)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EmployeeLeave>()
                .Property(el => el.UsedDays)
                .HasPrecision(18, 2);

            // Configure LeaveRequest relationships and decimal precision
            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(lr => lr.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure LeaveRequest approval chain relationships
            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.TeamLeader)
                .WithMany()
                .HasForeignKey(lr => lr.TeamLeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.Director)
                .WithMany()
                .HasForeignKey(lr => lr.DirectorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.HR)
                .WithMany()
                .HasForeignKey(lr => lr.HRId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveRequest>()
                .Property(lr => lr.TotalDays)
                .HasPrecision(18, 2);

            // Configure Payroll decimal precision
            modelBuilder.Entity<Payroll>()
                .Property(p => p.AttendanceDeduction)
                .HasPrecision(18, 2);

            // Configure PerformanceReview relationships with NoAction delete behavior
            modelBuilder.Entity<PerformanceReview>()
                .HasOne(pr => pr.Employee)
                .WithMany(e => e.PerformanceReviews)
                .HasForeignKey(pr => pr.EmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PerformanceReview>()
                .HasOne(pr => pr.Reviewer)
                .WithMany()
                .HasForeignKey(pr => pr.ReviewerId)
                .OnDelete(DeleteBehavior.NoAction);

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

            modelBuilder.Entity<PayrollConfiguration>().HasData(
                new PayrollConfiguration
                {
                    Id = 1,
                    EmployeePensionRate = 7,
                    EmployerPensionRate = 11,
                    LateDeductionRate = 0.25m,
                    AbsentDeductionRate = 1.0m
                }
            );

            modelBuilder.Entity<DeductionType>().HasData(
                new DeductionType { Id = 1, Name = "CostSharing", DisplayName = "Cost Sharing", Order = 1 },
                new DeductionType { Id = 2, Name = "SomeContribution", DisplayName = "Some Contribution", Order = 2 },
                new DeductionType { Id = 3, Name = "Saving", DisplayName = "Saving", Order = 3 },
                new DeductionType { Id = 4, Name = "HIV", DisplayName = "HIV", Order = 4 },
                new DeductionType { Id = 5, Name = "DefenceForce", DisplayName = "Defence Force", Order = 5 },
                new DeductionType { Id = 6, Name = "Health", DisplayName = "Health", Order = 6 },
                new DeductionType { Id = 7, Name = "ProsperityParty", DisplayName = "Prosperity Party", Order = 7 },
                new DeductionType { Id = 8, Name = "ReturnFromSalary", DisplayName = "Return from Salary", Order = 8 },
                new DeductionType { Id = 9, Name = "RedCross", DisplayName = "Red Cross", Order = 9 }
            );
        }

        // Ensure SaveChangesAsync is available for async operations
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
 
} 