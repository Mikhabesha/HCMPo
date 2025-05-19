using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public class EmployeeEducation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Institution { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Degree { get; set; }
        
        [StringLength(100)]
        public string FieldOfStudy { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
        
        public bool IsCompleted { get; set; }
        
        [StringLength(10)]
        public string Grade { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [StringLength(100)]
        public string Location { get; set; }
        
        public bool IsVerified { get; set; }
        
        public DateTime? VerificationDate { get; set; }
        
        [StringLength(100)]
        public string VerifiedBy { get; set; }
    }
} 