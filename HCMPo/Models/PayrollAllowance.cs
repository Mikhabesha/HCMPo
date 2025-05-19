using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public class PayrollAllowance
    {
        public int Id { get; set; }

        [Required]
        public string PayrollId { get; set; }
        public Payroll Payroll { get; set; }

        [Required]
        public int AllowanceTypeId { get; set; }
        public AllowanceType AllowanceType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
    }
} 