using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public class EmployeeDocument
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string EmployeeId { get; set; } = string.Empty;
        public virtual Employee Employee { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        public string FilePath { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public DateTime UploadDate { get; set; }
        
        [Required]
        public string UploadedBy { get; set; } = string.Empty;
        
        public DateTime? ExpiryDate { get; set; }
        
        public bool IsActive { get; set; } = true;

        public static readonly string[] DocumentTypeOptions = new[]
        {
            "Profile Photo", "ID", "Contract", "Certificate", "Resume", "Appraisal", "Other"
        };
    }
} 