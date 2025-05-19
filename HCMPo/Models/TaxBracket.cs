using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public class TaxBracket
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(100)]
        public string TaxName { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MinSalary { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxSalary { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Percentage { get; set; }

        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal Rate { get; set; }
        public decimal Deduction { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }
} 