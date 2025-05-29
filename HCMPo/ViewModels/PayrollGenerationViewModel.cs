using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HCMPo.Models;

namespace HCMPo.ViewModels
{
    public class PayrollGenerationViewModel
    {
        [Required]
        public string EmployeeId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasicSalary { get; set; }

        public List<AllowanceItem> Allowances { get; set; } = new List<AllowanceItem>();
        public List<DeductionItem> Deductions { get; set; } = new List<DeductionItem>();
    }

    public class AllowanceItem
    {
        public int AllowanceTypeId { get; set; }
        public string AllowanceTypeName { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
    }

    public class DeductionItem
    {
        public int DeductionTypeId { get; set; }
        public string DeductionTypeName { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
    }
} 