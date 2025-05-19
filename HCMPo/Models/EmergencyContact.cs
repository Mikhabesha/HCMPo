using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public class EmergencyContact
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Relationship { get; set; }
        
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        
        [Phone]
        public string AlternativePhoneNumber { get; set; }
        
        [StringLength(200)]
        public string Address { get; set; }
        
        [StringLength(100)]
        public string City { get; set; }
        
        public bool IsPrimary { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }
    }
} 