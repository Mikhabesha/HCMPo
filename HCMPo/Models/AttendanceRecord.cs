using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public class AttendanceRecord
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        [DataType(DataType.Time)]
        public DateTime? CheckInTime { get; set; }

        [DataType(DataType.Time)]
        public DateTime? CheckOutTime { get; set; }

        public string Remarks { get; set; }

        // For tracking changes
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
    }
} 