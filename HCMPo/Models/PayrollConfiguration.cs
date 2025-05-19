using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models
{
    public class PayrollConfiguration
    {
        [Key]
        public int Id { get; set; }
        public decimal EmployeePensionRate { get; set; } = 7;
        public decimal EmployerPensionRate { get; set; } = 11;
        public decimal LateDeductionRate { get; set; } = 0.25m; // 25% of daily rate
        public decimal AbsentDeductionRate { get; set; } = 1.0m; // 100% of daily rate
    }
} 