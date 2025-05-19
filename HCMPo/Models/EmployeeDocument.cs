using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public class EmployeeDocument
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; }
        
        [Required]
        [StringLength(255)]
        public string FileName { get; set; }
        
        [Required]
        public string FilePath { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        public DateTime UploadDate { get; set; }
        
        [Required]
        public string UploadedBy { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
} 