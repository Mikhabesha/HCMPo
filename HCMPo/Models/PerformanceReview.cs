using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public class PerformanceReview
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }
        
        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Review Date")]
        public DateTime ReviewDate { get; set; }

        [Required]
        [Display(Name = "Review Period Start")]
        [DataType(DataType.Date)]
        public DateTime PeriodStart { get; set; }

        [Required]
        [Display(Name = "Review Period End")]
        [DataType(DataType.Date)]
        public DateTime PeriodEnd { get; set; }

        [Required]
        [Range(1, 5)]
        [Display(Name = "Performance Rating")]
        public int PerformanceRating { get; set; }

        [Required]
        [StringLength(1000)]
        [Display(Name = "Achievements")]
        public string Achievements { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        [Display(Name = "Areas for Improvement")]
        public string AreasForImprovement { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Goals for Next Period")]
        public string? GoalsForNextPeriod { get; set; }

        [StringLength(1000)]
        [Display(Name = "Manager Comments")]
        public string? ManagerComments { get; set; }

        [StringLength(1000)]
        [Display(Name = "Employee Comments")]
        public string? EmployeeComments { get; set; }

        [StringLength(450)]
        [Display(Name = "Reviewed By")]
        public string? ReviewedById { get; set; } // UserId of the reviewer

        [Display(Name = "Is Acknowledged")]
        public bool IsAcknowledged { get; set; } = false;

        [DataType(DataType.Date)]
        [Display(Name = "Acknowledgement Date")]
        public DateTime? AcknowledgementDate { get; set; }
    }
} 