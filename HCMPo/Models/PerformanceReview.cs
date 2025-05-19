using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public class PerformanceReview
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string EmployeeId { get; set; }  // Changed from int to string

        [Required]
        public virtual Employee Employee { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReviewDate { get; set; }

        [Required]
        [Range(1, 5)]
        public int Performance { get; set; }

        [Required]
        [Range(1, 5)]
        public int Punctuality { get; set; }

        [Required]
        [Range(1, 5)]
        public int Teamwork { get; set; }

        [Required]
        [Range(1, 5)]
        public int Communication { get; set; }

        [Required]
        public string Comments { get; set; }

        [Required]
        public string ReviewerId { get; set; }

        public virtual Employee Reviewer { get; set; }

        public string Status { get; set; } = "Pending";
    }
} 