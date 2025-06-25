namespace HCMPo.Models.ViewModels
{
    public class PayrollReportViewModel
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string BadgeNumber { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal TotalAllowance { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal PensionEmployer { get; set; } // 11%
        public decimal IncomeTax { get; set; }
        public decimal PensionEmployee { get; set; } // 7%
        public List<DeductionViewModel> Deductions { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
    }

    public class DeductionViewModel
    {
        public string DeductionName { get; set; }
        public decimal Amount { get; set; }
    }
} 