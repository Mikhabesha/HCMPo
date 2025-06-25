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
        public DbSet<AllowanceType> AllowanceTypes { get; set; }
        public DbSet<EmployeeAllowance> EmployeeAllowances { get; set; }
        public DbSet<EmployeeDeduction> EmployeeDeductions { get; set; }
        public DbSet<EmployeeLeaveEntitlement> EmployeeLeaveEntitlements { get; set; }
        public DbSet<LeaveCarryover> LeaveCarryovers { get; set; }
        public DbSet<PayrollStamp> PayrollStamps { get; set; }
        public DbSet<PayrollSlipConfiguration> PayrollSlipConfigurations { get; set; }
        public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
        
        // New organization structure models
        public DbSet<Directorate> Directorates { get; set; }
        public DbSet<RoleHierarchy> RoleHierarchies { get; set; }
        public DbSet<EmployeePosition> EmployeePositions { get; set; }
        public DbSet<EmployeeMigrationLog> EmployeeMigrationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure one-to-one relationship between Employee and ApplicationUser
            // Only use ApplicationUser.EmployeeId as the foreign key
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Employee)
                .WithOne(e => e.ApplicationUser)
                .HasForeignKey<ApplicationUser>(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ignore the UserId and ApplicationUserId properties on Employee to prevent shadow FK creation
            modelBuilder.Entity<Employee>()
                .Ignore(e => e.UserId);
                //.Ignore(e => e.ApplicationUserId);

            // Configure Employee relationships
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

            // Ignore the UserId and ApplicationUserId properties on Employee to prevent shadow FK creation
            modelBuilder.Entity<Employee>()
                .Ignore(e => e.UserId);
                //.Ignore(e => e.ApplicationUserId);

            // Configure Employee decimal precision
            modelBuilder.Entity<Employee>()
                .Property(e => e.Salary)
                .HasPrecision(18, 2);

            // Ignore the UserId and ApplicationUserId properties on Employee to prevent shadow FK creation
            modelBuilder.Entity<Employee>()
                .Ignore(e => e.UserId);
                //.Ignore(e => e.ApplicationUserId);

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

            // Configure EmployeeLeaveEntitlement relationships
            modelBuilder.Entity<EmployeeLeaveEntitlement>()
                .HasOne(ele => ele.Employee)
                .WithMany()
                .HasForeignKey(ele => ele.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeLeaveEntitlement>()
                .HasOne(ele => ele.LeaveType)
                .WithMany()
                .HasForeignKey(ele => ele.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeLeaveEntitlement>()
                .Property(ele => ele.CustomEntitlementDays)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EmployeeLeaveEntitlement>()
                .Property(ele => ele.BaseEntitlement)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EmployeeLeaveEntitlement>()
                .Property(ele => ele.AnnualIncrement)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EmployeeLeaveEntitlement>()
                .Property(ele => ele.MaxEntitlement)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EmployeeLeaveEntitlement>()
                .Property(ele => ele.MaxCarryoverDays)
                .HasPrecision(18, 2);

            // Configure LeaveCarryover relationships
            modelBuilder.Entity<LeaveCarryover>()
                .HasOne(lc => lc.Employee)
                .WithMany()
                .HasForeignKey(lc => lc.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveCarryover>()
                .HasOne(lc => lc.LeaveType)
                .WithMany()
                .HasForeignKey(lc => lc.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveCarryover>()
                .HasOne(lc => lc.ApprovedByEmployee)
                .WithMany()
                .HasForeignKey(lc => lc.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveCarryover>()
                .Property(lc => lc.AvailableDays)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LeaveCarryover>()
                .Property(lc => lc.CarriedOverDays)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LeaveCarryover>()
                .Property(lc => lc.ExpiredDays)
                .HasPrecision(18, 2);

            // Enhanced EmployeeLeave configurations
            modelBuilder.Entity<EmployeeLeave>()
                .Property(el => el.CarryoverDays)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EmployeeLeave>()
                .Property(el => el.MaxCarryoverDays)
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

            // Configure Employee-EmployeeDocument relationship
            modelBuilder.Entity<EmployeeDocument>()
                .HasOne(d => d.Employee)
                .WithMany(e => e.Documents)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed initial data
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

            // Configure new organization structure models
            modelBuilder.Entity<Directorate>()
                .HasOne(d => d.ParentDirectorate)
                .WithMany(d => d.SubDirectorates)
                .HasForeignKey(d => d.ParentDirectorateId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeePosition>()
                .HasOne(ep => ep.Employee)
                .WithMany()
                .HasForeignKey(ep => ep.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeePosition>()
                .HasOne(ep => ep.Directorate)
                .WithMany()
                .HasForeignKey(ep => ep.DirectorateId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeePosition>()
                .HasOne(ep => ep.OrganizationUnit)
                .WithMany()
                .HasForeignKey(ep => ep.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeePosition>()
                .HasOne(ep => ep.RoleHierarchy)
                .WithMany()
                .HasForeignKey(ep => ep.RoleHierarchyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeePosition>()
                .HasOne(ep => ep.SupervisorEmployee)
                .WithMany()
                .HasForeignKey(ep => ep.SupervisorEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial directorates
            modelBuilder.Entity<Directorate>().HasData(
                new Directorate { Id = "1", Name = "Office of the Director General", Code = "OCG", Level = 1 },
                new Directorate { Id = "2", Name = "Operations Management Directorate", Code = "RMD", Level = 1 },
                new Directorate { Id = "3", Name = "Member Services Directorate", Code = "COD", Level = 1 },
                new Directorate { Id = "4", Name = "Benefits Administration Directorate", Code = "TAD", Level = 1 },
                new Directorate { Id = "5", Name = "Investigation and Intelligence Directorate", Code = "IID", Level = 1 },
                new Directorate { Id = "6", Name = "Legal Affairs Directorate", Code = "LAD", Level = 1 },
                new Directorate { Id = "7", Name = "Internal Audit Directorate", Code = "IAD", Level = 1 },
                new Directorate { Id = "8", Name = "Human Resources Directorate", Code = "HRD", Level = 1 },
                new Directorate { Id = "9", Name = "Finance and Procurement Directorate", Code = "FPD", Level = 1 },
                new Directorate { Id = "10", Name = "Information Technology Directorate", Code = "ITD", Level = 1 },
                new Directorate { Id = "11", Name = "Planning and Performance Directorate", Code = "PPD", Level = 1 },
                new Directorate { Id = "12", Name = "Corporate Communication Directorate", Code = "CCD", Level = 1 },
                new Directorate { Id = "13", Name = "Policy and Research Directorate", Code = "RID", Level = 1 },
                new Directorate { Id = "14", Name = "Regional Operations Directorate", Code = "ROD", Level = 1 }
            );

            // Seed initial role hierarchies
            modelBuilder.Entity<RoleHierarchy>().HasData(
                new RoleHierarchy { Id = "1", RoleName = "Employee", DisplayName = "Employee", HierarchyLevel = 1, MaxApprovalLevel = 0, CanApproveLeave = false, CanManageAttendance = false, CanAccessPayroll = false, CanManageEmployees = false },
                new RoleHierarchy { Id = "2", RoleName = "TeamLeader", DisplayName = "Team Leader", HierarchyLevel = 2, MaxApprovalLevel = 1, CanApproveLeave = true, CanManageAttendance = true, CanAccessPayroll = false, CanManageEmployees = false },
                new RoleHierarchy { Id = "3", RoleName = "HR", DisplayName = "Human Resources", HierarchyLevel = 3, MaxApprovalLevel = 2, CanApproveLeave = true, CanManageAttendance = true, CanAccessPayroll = true, CanManageEmployees = true },
                new RoleHierarchy { Id = "4", RoleName = "Director", DisplayName = "Director", HierarchyLevel = 4, MaxApprovalLevel = 3, CanApproveLeave = true, CanManageAttendance = true, CanAccessPayroll = true, CanManageEmployees = true },
                new RoleHierarchy { Id = "5", RoleName = "Admin", DisplayName = "Administrator", HierarchyLevel = 5, MaxApprovalLevel = 5, CanApproveLeave = true, CanManageAttendance = true, CanAccessPayroll = true, CanManageEmployees = true }
            );
        }

        // Ensure SaveChangesAsync is available for async operations
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
 
} 