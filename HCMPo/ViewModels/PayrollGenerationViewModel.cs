using System;
using System.Collections.Generic;
using HCMPo.Models;

namespace HCMPo.ViewModels
{
    public class PayrollGenerationViewModel
    {
        public string EmployeeId { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Allowance { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Dictionary<DeductionType, decimal> Deductions { get; set; } = new();
    }
} 