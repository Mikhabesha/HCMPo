using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models
{
    public class LeaveType
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public int DefaultDays { get; set; }
        public bool IsPaidLeave { get; set; }
        public bool RequiresAttachment { get; set; }
        public bool RequiresApproval { get; set; }
        public int MaxDaysPerRequest { get; set; }
        public bool AllowHalfDay { get; set; }
        public bool IsActive { get; set; } = true;
    }
} 