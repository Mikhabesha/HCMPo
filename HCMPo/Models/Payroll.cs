using System;
using System.Collections.Generic;
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

        public DateTime? SentDate { get; set; }
        public bool IsOpened { get; set; }

        public string? AppliedTaxesSummary { get; set; }

        public decimal AttendanceDeductedSalary { get; set; }
        public decimal Allowance { get; set; }
        public decimal TotalSalary { get; set; }
        public decimal PensionEmployee { get; set; }
        public decimal PensionEmployer { get; set; } // Display only
        public decimal TotalDeduction { get; set; }
        public decimal NetPaySalary { get; set; }
        public virtual ICollection<PayrollDeduction> Deductions { get; set; }

        // Navigation properties
        public virtual ICollection<PayrollAllowance> Allowances { get; set; }
    }

    public enum PayrollStatus
    {
        Draft,
        Generated,
        Approved,
        Paid,
        Cancelled,
        NeedsReview
    }
} 