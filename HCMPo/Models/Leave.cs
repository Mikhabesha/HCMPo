using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HCMPo.Models.Enums;

namespace HCMPo.Models
{
    public class EmployeeLeave
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Required]
        public string LeaveTypeId { get; set; }
        public virtual LeaveType LeaveType { get; set; }

        public int Year { get; set; }
        public decimal TotalDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal RemainingDays { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class LeaveRequest
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Required]
        public string LeaveTypeId { get; set; }
        public virtual LeaveType LeaveType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        public string StartDateEt { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
        public string EndDateEt { get; set; }

        public decimal TotalDays { get; set; }
        public bool IsHalfDay { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        public string AttachmentUrl { get; set; }
        
        public LeaveRequestStatus Status { get; set; }
        public string CurrentApprover { get; set; }

        // For tracking the approval chain
        public string TeamLeaderId { get; set; }
        public virtual Employee TeamLeader { get; set; }
        public DateTime? TeamLeaderApprovalDate { get; set; }
        public string TeamLeaderRemarks { get; set; }

        public string DirectorId { get; set; }
        public virtual Employee Director { get; set; }
        public DateTime? DirectorApprovalDate { get; set; }
        public string DirectorRemarks { get; set; }

        public string HRId { get; set; }
        public virtual Employee HR { get; set; }
        public DateTime? HRApprovalDate { get; set; }
        public string HRRemarks { get; set; }

        // Tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class LeaveConfiguration
    {
        public bool RequireTeamLeaderApproval { get; set; } = true;
        public bool RequireDirectorApproval { get; set; } = true;
        public bool RequireHRApproval { get; set; } = true;
        public int MaxConsecutiveLeaveDays { get; set; } = 30;
        public bool AllowLeaveWithoutBalance { get; set; } = false;
        public bool AutomaticallyResetLeaveBalance { get; set; } = true;
        public int LeaveYearStartMonth { get; set; } = 1; // January
        public bool UseEthiopianCalendar { get; set; } = true;
    }

    public class Leave
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        [Required]
        public string LeaveTypeId { get; set; }
        public virtual LeaveType? LeaveType { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public string? StartDateEt { get; set; }
        public string? EndDateEt { get; set; }

        [Required]
        public int TotalDays { get; set; }

        [Required]
        public LeaveStatus Status { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public string? RejectedBy { get; set; }
        public DateTime? RejectedAt { get; set; }

        public string? AttachmentPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled
    }
} 