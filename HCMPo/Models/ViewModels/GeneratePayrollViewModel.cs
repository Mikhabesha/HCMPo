using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HCMPo.Models.ViewModels
{
    public class GeneratePayrollViewModel
    {
        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Start Date (ET)")]
        public string StartDateEt { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [Display(Name = "End Date (ET)")]
        public string EndDateEt { get; set; }

        [Display(Name = "Select Employees")]
        public List<string> SelectedEmployeeIds { get; set; }

        public List<SelectListItem> EmployeeList { get; set; }
    }
} 