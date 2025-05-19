using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models
{
    public class LeaveType
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int DefaultDays { get; set; }

        public bool IsPaid { get; set; } = true;

        public bool RequiresApproval { get; set; } = true;

        public bool RequiresAttachment { get; set; }

        public int? MinimumNoticeDays { get; set; }

        public int? MaximumConsecutiveDays { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsPaidLeave { get; set; } = true;
        public int MaxDaysPerRequest { get; set; } = 30;
        public bool AllowHalfDay { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }

        public virtual ICollection<Leave> Leaves { get; set; } = new List<Leave>();
    }
} 