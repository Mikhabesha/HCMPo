using System;
using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models
{
    // Individual employee leave balance for a specific year
    public class EmployeeLeave
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Required]
        public string LeaveTypeId { get; set; }
        public virtual LeaveType LeaveType { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public decimal TotalDays { get; set; }

        public decimal UsedDays { get; set; } = 0;

        public decimal RemainingDays { get; set; }

        // Carryover from previous year (managed by HR)
        public decimal CarryoverDays { get; set; } = 0;
        
        // Maximum days that can be carried over
        public decimal MaxCarryoverDays { get; set; } = 5;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }

    // HR-managed individual employee leave entitlements (overrides defaults)
    public class EmployeeLeaveEntitlement
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Required]
        public string LeaveTypeId { get; set; }
        public virtual LeaveType LeaveType { get; set; }

        // Custom entitlement for this employee (overrides default calculation)
        public decimal? CustomEntitlementDays { get; set; }

        // Whether to use years of service calculation or custom entitlement
        public bool UseCustomEntitlement { get; set; } = false;

        // Base entitlement (used for annual leave calculation)
        public decimal BaseEntitlement { get; set; } = 20;

        // Annual increment (used for annual leave)
        public decimal AnnualIncrement { get; set; } = 1;

        // Maximum entitlement cap
        public decimal MaxEntitlement { get; set; } = 30;

        // Maximum carryover allowed for this employee/leave type
        public decimal MaxCarryoverDays { get; set; } = 5;

        // Whether carryover is allowed for this employee/leave type
        public bool AllowCarryover { get; set; } = true;

        // Effective date for this entitlement
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

        // Expiry date (null = permanent)
        public DateTime? ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public string? Notes { get; set; }
    }

    // Leave carryover transactions (audit trail)
    public class LeaveCarryover
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Required]
        public string LeaveTypeId { get; set; }
        public virtual LeaveType LeaveType { get; set; }

        public int FromYear { get; set; }
        public int ToYear { get; set; }

        public decimal AvailableDays { get; set; }
        public decimal CarriedOverDays { get; set; }
        public decimal ExpiredDays { get; set; }

        public string? ApprovedBy { get; set; }
        public virtual Employee? ApprovedByEmployee { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
    }
} 