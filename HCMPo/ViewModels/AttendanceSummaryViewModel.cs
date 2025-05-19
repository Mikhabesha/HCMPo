using System.ComponentModel.DataAnnotations;

namespace HCMPo.ViewModels
{
    // ViewModel for the Attendance Summary Query Form
    public class AttendanceSummaryQueryViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today.AddMonths(-1).AddDays(1 - DateTime.Today.Day); // Default to first day of last month

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(-DateTime.Today.Day); // Default to last day of last month

        [Display(Name = "Select Specific Employees (Optional)")]
        public List<string>? SelectedEmployeeIds { get; set; }
    }

    // ViewModel to hold the calculated summary for one employee
    public class EmployeeAttendanceSummaryViewModel
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int TotalWorkingDaysInPeriod { get; set; }
        public double DaysPresent { get; set; }
        public double DaysAbsent { get; set; }
        public int DaysLate { get; set; }
        public int DaysEarlyDeparture { get; set; }
        public int DaysOnLeave { get; set; }
        public int DaysOnField { get; set; }
        public int DaysHoliday { get; set; }
        public int DaysWeekend { get; set; }
        public TimeSpan TotalWorkDuration { get; set; }

        public EmployeeAttendanceSummaryViewModel()
        {
            TotalWorkingDaysInPeriod = 0;
            DaysPresent = 0;
            DaysAbsent = 0;
            DaysLate = 0;
            DaysEarlyDeparture = 0;
            DaysOnLeave = 0;
            DaysOnField = 0;
            DaysHoliday = 0;
            DaysWeekend = 0;
            TotalWorkDuration = TimeSpan.Zero;
        }
    }
} 