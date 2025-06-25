using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HCMPo.ViewModels
{
    public class EmployeeDocumentUploadViewModel
    {
        [Required]
        public string EmployeeId { get; set; }

        [Required]
        [Display(Name = "Document Type")]
        public string DocumentType { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        [Required]
        [Display(Name = "Document File")]
        public IFormFile DocumentFile { get; set; }
    }
} 