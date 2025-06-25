using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HCMPo.Models;
using HCMPo.Services;

namespace HCMPo.Models.ViewModels
{
    public class PayrollSlipViewModel
    {
        public string OrganizationUnitId { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime PayrollPeriodStart { get; set; }
        public DateTime PayrollPeriodEnd { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal OvertimeRate { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal Bonus { get; set; }
        public decimal Commission { get; set; }
        public decimal OtherIncome { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal OtherPayments { get; set; }
        public decimal TaxableIncome { get; set; }
        public decimal Tax { get; set; }
        public decimal NetPay { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal TotalNetPay { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalTaxableIncome { get; set; }
        public decimal TotalOtherIncome { get; set; }
        public decimal TotalOtherDeductions { get; set; }
        public decimal TotalOtherPayments { get; set; }
        public decimal TotalNetPayments { get; set; }
        public decimal TotalNetDeductions { get; set; }
        public decimal TotalNetTax { get; set; }
        public decimal TotalNetTaxableIncome { get; set; }
        public decimal TotalNetOtherIncome { get; set; }
        public decimal TotalNetOtherDeductions { get; set; }
        public decimal TotalNetOtherPayments { get; set; }
        public decimal TotalNetOtherNetPayments { get; set; }
        public decimal TotalNetOtherNetDeductions { get; set; }
        public decimal TotalNetOtherNetTax { get; set; }
        public decimal TotalNetOtherNetTaxableIncome { get; set; }
        public decimal TotalNetOtherNetOtherIncome { get; set; }
        public decimal TotalNetOtherNetOtherDeductions { get; set; }
        public decimal TotalNetOtherNetOtherPayments { get; set; }
        public decimal TotalNetOtherNetOtherNetPayments { get; set; }
        public decimal TotalNetOtherNetOtherNetDeductions { get; set; }
        public decimal TotalNetOtherNetOtherNetTax { get; set; }
        public decimal TotalNetOtherNetOtherNetTaxableIncome { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherIncome { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherDeductions { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherPayments { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherNetPayments { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherNetDeductions { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherNetTax { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherNetTaxableIncome { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherNetOtherIncome { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherNetOtherDeductions { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherNetOtherPayments { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherNetOtherNetPayments { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherNetOtherNetDeductions { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherNetOtherNetTax { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherNetOtherNetTaxableIncome { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherNetOtherNetOtherIncome { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherNetOtherNetOtherDeductions { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherNetOtherNetOtherPayments { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherNetOtherNetOtherNetPayments { get; set; }
        public decimal TotalNetOtherNetOtherNetOtherNetOtherNetOtherNetDeductions { get; set; }
    }
}
 