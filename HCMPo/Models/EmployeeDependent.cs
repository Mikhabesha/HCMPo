using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public class EmployeeDependent
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
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        
        [StringLength(20)]
        public string Gender { get; set; }
        
        [StringLength(50)]
        public string Nationality { get; set; }
        
        [StringLength(50)]
        public string IdentificationNumber { get; set; }
        
        [StringLength(200)]
        public string Address { get; set; }
        
        [StringLength(100)]
        public string City { get; set; }
        
        [Phone]
        public string PhoneNumber { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
} 