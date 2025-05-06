using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public class Attendance
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Required]
        public DateTime CheckInTime { get; set; }
        public string CheckInTimeEt { get; set; } // Ethiopian Calendar

        public DateTime? CheckOutTime { get; set; }
        public string CheckOutTimeEt { get; set; } // Ethiopian Calendar

        public AttendanceStatus Status { get; set; }
        public string StatusReason { get; set; }

        // ZKTeco specific fields
        public string DeviceId { get; set; }
        public string PunchType { get; set; }
        public string VerificationMode { get; set; }
        public string WorkCode { get; set; }

        // Calculated fields
        [NotMapped]
        public TimeSpan? WorkDuration => CheckOutTime.HasValue ? CheckOutTime.Value - CheckInTime : null;

        public bool IsLate { get; set; }
        public bool IsEarlyDeparture { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }

        // For manual adjustments/corrections
        public bool IsManualEntry { get; set; }
        public string ManualEntryReason { get; set; }
        public string ApprovedBy { get; set; }
    }

    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late,
        EarlyDeparture,
        OnField,
        OnLeave,
        Holiday,
        Weekend
    }

    // Configuration class for attendance rules
    public class AttendanceConfiguration
    {
        public TimeSpan WorkDayStart { get; set; } = new TimeSpan(9, 0, 0); // 9:00 AM
        public TimeSpan WorkDayEnd { get; set; } = new TimeSpan(17, 0, 0); // 5:00 PM
        public TimeSpan LateThreshold { get; set; } = new TimeSpan(0, 15, 0); // 15 minutes
        public TimeSpan EarlyDepartureThreshold { get; set; } = new TimeSpan(0, 15, 0); // 15 minutes
        public bool IsWorkingDay(DayOfWeek day) => day != DayOfWeek.Saturday && day != DayOfWeek.Sunday;
    }
} 