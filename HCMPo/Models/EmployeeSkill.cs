using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public class EmployeeSkill
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }
        
        [Required]
        [StringLength(100)]
        public string SkillName { get; set; }
        
        [StringLength(50)]
        public string SkillLevel { get; set; } // Beginner, Intermediate, Advanced, Expert
        
        [StringLength(100)]
        public string CertificationName { get; set; }
        
        [StringLength(100)]
        public string IssuingOrganization { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? IssueDate { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }
        
        [StringLength(50)]
        public string CertificationNumber { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public bool IsVerified { get; set; }
        
        public DateTime? VerificationDate { get; set; }
        
        [StringLength(100)]
        public string VerifiedBy { get; set; }
    }
} 