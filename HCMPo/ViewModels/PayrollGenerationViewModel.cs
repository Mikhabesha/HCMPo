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
        public string EmployeeId { get; set; } = string.Empty;

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

        // Additional properties for PayrollController
        public List<string>? SelectedEmployeeIds { get; set; }
        public string StartDateEt { get; set; } = string.Empty;
        public string EndDateEt { get; set; } = string.Empty;
    }

    public class AllowanceItem
    {
        public int AllowanceTypeId { get; set; }
        public string AllowanceTypeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }

    public class DeductionItem
    {
        public int DeductionTypeId { get; set; }
        public string DeductionTypeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }
} 