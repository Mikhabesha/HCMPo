using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models.ViewModels
{
    public class EmployeeFilterViewModel
    {
        [Display(Name = "Search")]
        public string SearchTerm { get; set; }

        [Display(Name = "Department")]
        public string DepartmentId { get; set; }

        [Display(Name = "Job Title")]
        public string JobTitleId { get; set; }

        [Display(Name = "Status")]
        public EmploymentStatus? Status { get; set; }

        public IEnumerable<Employee> Employees { get; set; }
        public SelectList Departments { get; set; }
        public SelectList JobTitles { get; set; }
        public SelectList Statuses { get; set; }
    }
} 