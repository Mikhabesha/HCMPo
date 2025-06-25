using System.ComponentModel.DataAnnotations;
using HCMPo.Models;

namespace HCMPo.ViewModels
{
    public class PayrollSlipViewModel
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
        public string BadgeNumber { get; set; }
        public string OrganizationUnit { get; set; }
        public string Position { get; set; }
        public DateTime PayPeriodStart { get; set; }
        public DateTime PayPeriodEnd { get; set; }
        public string PayPeriodStartEt { get; set; }
        public string PayPeriodEndEt { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal IncomeTax { get; set; }
        public decimal PensionDeduction { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public int WorkingDays { get; set; }
        public decimal DaysWorked { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public string SlipType { get; set; } // "bank" or "report"
        public PayrollSlipConfiguration SlipConfiguration { get; set; }
        public PayrollStamp ActiveStamp { get; set; }
        public List<PayrollAllowance> DetailedAllowances { get; set; } = new List<PayrollAllowance>();
        public List<PayrollDeduction> DetailedDeductions { get; set; } = new List<PayrollDeduction>();
        public string OrganizationUnitId { get; set; }
    }

    public class PayrollSlipFilterViewModel
    {
        public string EmployeeId { get; set; }
        public string OrganizationUnitId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SlipType { get; set; } = "bank"; // "bank" or "report"
        public string ExportFormat { get; set; } = "pdf"; // "pdf", "excel", "csv"
        public bool ApplyStamp { get; set; } = true;
        public List<string> SelectedPayrollIds { get; set; } = new List<string>();
    }

    public class PayrollSlipBulkViewModel
    {
        public List<PayrollSlipViewModel> Slips { get; set; } = new List<PayrollSlipViewModel>();
        public PayrollSlipFilterViewModel Filter { get; set; }
        public int TotalCount { get; set; }
        public string ExportFileName { get; set; }
    }

    public class PayrollSummaryViewModel
    {
        public string Scope { get; set; } // "all" or "individual"
        public string Period { get; set; } // "monthly" or "annual"
        public string EmployeeId { get; set; }
        public string OrganizationUnitId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        
        // Summary data
        public decimal TotalGrossSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetSalary { get; set; }
        public int TotalEmployees { get; set; }
        public List<MonthlySummary> MonthlySummaries { get; set; } = new List<MonthlySummary>();
        public List<EmployeeSummary> EmployeeSummaries { get; set; } = new List<EmployeeSummary>();
    }

    public class MonthlySummary
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public int EmployeeCount { get; set; }
    }

    public class EmployeeSummary
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string BadgeNumber { get; set; }
        public string OrganizationUnit { get; set; }
        public decimal TotalGross { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNet { get; set; }
        public int PayrollCount { get; set; }
    }

    public class StampUploadViewModel
    {
        [Required]
        [Display(Name = "Stamp Image")]
        public IFormFile StampFile { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Apply on Bank Slip")]
        public bool ApplyOnBankSlip { get; set; } = true;

        [Display(Name = "Apply on Report Slip")]
        public bool ApplyOnReportSlip { get; set; } = true;

        [Range(50, 500)]
        [Display(Name = "Width (pixels)")]
        public int Width { get; set; } = 150;

        [Range(25, 250)]
        [Display(Name = "Height (pixels)")]
        public int Height { get; set; } = 75;

        [Display(Name = "Position")]
        public string Position { get; set; } = "bottom-right";

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
} 