using System.ComponentModel.DataAnnotations;

namespace HCMPo.ViewModels
{
    public class GeneratePayrollViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Pay Period Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today.AddMonths(-1).AddDays(1 - DateTime.Today.Day); // Default to first day of last month

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Pay Period End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(-DateTime.Today.Day); // Default to last day of last month

        [Display(Name = "Select Specific Employees (Optional)")]
        public List<string>? SelectedEmployeeIds { get; set; } // For selecting specific employees

        // We might add other options later, like selecting by department
    }
} 