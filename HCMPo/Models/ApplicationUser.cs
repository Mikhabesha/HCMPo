using Microsoft.AspNetCore.Identity;

namespace HCMPo.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        // Additional properties for user preferences
        public string? Theme { get; set; }
        public string? Language { get; set; }
        public bool UseEthiopianCalendar { get; set; } = true;

        // Notification preferences
        public bool EmailNotifications { get; set; } = true;
        public bool AttendanceAlerts { get; set; } = true;
        public bool LeaveRequestAlerts { get; set; } = true;
        public bool PayrollAlerts { get; set; } = true;

        // Last login tracking
        public DateTime? LastLoginDate { get; set; }
        public string? LastLoginIP { get; set; }
    }

    public static class ApplicationRoles
    {
        public const string Admin = "Admin";
        public const string HR = "HR";
        public const string Employee = "Employee";
        public const string TeamLeader = "TeamLeader";
        public const string Director = "Director";

        public static readonly string[] AllRoles = new[]
        {
            Admin,
            HR,
            Employee,
            TeamLeader,
            Director
        };
    }
} 