using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public class AttendanceRule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public int GracePeriodMinutes { get; set; } = 15; // Default 15 minutes grace period
        
        [Required]
        public int LateThresholdMinutes { get; set; } = 30; // Default 30 minutes late threshold
        
        [Required]
        public int EarlyLeaveThresholdMinutes { get; set; } = 30; // Default 30 minutes early leave threshold
        
        [Required]
        public int HalfDayThresholdMinutes { get; set; } = 240; // Default 4 hours for half day
        
        [Required]
        public bool AllowEarlyCheckIn { get; set; } = true;
        
        [Required]
        public bool AllowLateCheckOut { get; set; } = true;
        
        [Required]
        public int MaxEarlyCheckInMinutes { get; set; } = 60; // Default 1 hour before shift
        
        [Required]
        public int MaxLateCheckOutMinutes { get; set; } = 60; // Default 1 hour after shift
        
        [Required]
        public int MaxLateCountPerMonth { get; set; } = 3;
        
        [Required]
        public int MaxEarlyLeaveCountPerMonth { get; set; } = 3;
        
        [Required]
        public int MaxAbsenceCountPerMonth { get; set; } = 3;
        
        [Required]
        public bool RequiresApprovalForLate { get; set; } = true;
        
        [Required]
        public bool RequiresApprovalForEarlyLeave { get; set; } = true;
        
        [Required]
        public bool RequiresApprovalForAbsence { get; set; } = true;
        
        [Required]
        public bool IsActive { get; set; } = true;
        
        public virtual ICollection<Employee> Employees { get; set; }
        
        public AttendanceRule()
        {
            Employees = new HashSet<Employee>();
        }
    }
} 