using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models.ViewModels
{
    public class EmployeeFilterViewModel
    {
        [Display(Name = "Search")]
        public string SearchTerm { get; set; } = string.Empty;

        [Display(Name = "Department")]
        public string OrganizationUnitId { get; set; } = string.Empty;

        [Display(Name = "Job Title")]
        public string JobTitleId { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public EmploymentStatus? Status { get; set; }

        public IEnumerable<Employee> Employees { get; set; }
        public SelectList OrganizationUnits { get; set; }
        public SelectList JobTitles { get; set; }
        public SelectList Statuses { get; set; }
    }
} 