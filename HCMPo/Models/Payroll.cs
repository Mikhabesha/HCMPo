using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public class Payroll
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Required]
        public DateTime PayPeriodStart { get; set; }
        public string PayPeriodStartEt { get; set; }

        [Required]
        public DateTime PayPeriodEnd { get; set; }
        public string PayPeriodEndEt { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasicSalary { get; set; }

        // Allowances
        [Column(TypeName = "decimal(18,2)")]
        public decimal TransportAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HousingAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherAllowances { get; set; }

        // Deductions
        [Column(TypeName = "decimal(18,2)")]
        public decimal IncomeTax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PensionDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherDeductions { get; set; }

        // Attendance based calculations
        public int WorkingDays { get; set; }
        public int DaysWorked { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public decimal AttendanceDeduction { get; set; }

        // Totals
        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSalary { get; set; }

        // Status
        public PayrollStatus Status { get; set; }
        public string GeneratedBy { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string Remarks { get; set; }

        // For tracking changes
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
    }

    public enum PayrollStatus
    {
        Draft,
        Generated,
        Approved,
        Paid,
        Cancelled
    }

    public class PayrollConfiguration
    {
        // Tax brackets (can be configured from settings)
        public List<TaxBracket> TaxBrackets { get; set; } = new List<TaxBracket>
        {
            new TaxBracket { From = 0, To = 600, Rate = 0 },
            new TaxBracket { From = 601, To = 1650, Rate = 10 },
            new TaxBracket { From = 1651, To = 3200, Rate = 15 },
            new TaxBracket { From = 3201, To = 5250, Rate = 20 },
            new TaxBracket { From = 5251, To = 7800, Rate = 25 },
            new TaxBracket { From = 7801, To = 10900, Rate = 30 },
            new TaxBracket { From = 10901, To = decimal.MaxValue, Rate = 35 }
        };

        // Pension rates
        public decimal EmployeePensionRate { get; set; } = 7;
        public decimal EmployerPensionRate { get; set; } = 11;

        // Attendance deduction rules
        public decimal LateDeductionRate { get; set; } = 0.25m; // 25% of daily rate
        public decimal AbsentDeductionRate { get; set; } = 1.0m; // 100% of daily rate
    }

    public class TaxBracket
    {
        public decimal From { get; set; }
        public decimal To { get; set; }
        public decimal Rate { get; set; }
    }
} 