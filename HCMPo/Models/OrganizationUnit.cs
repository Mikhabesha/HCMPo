using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public enum OrganizationUnitType
    {
        HeadOffice,
        District,
        Branch,
        Department
    }

    public class OrganizationUnit
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public OrganizationUnitType Type { get; set; }

        public string? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public virtual OrganizationUnit? Parent { get; set; }
        public virtual ICollection<OrganizationUnit> Children { get; set; } = new HashSet<OrganizationUnit>();

        public bool IsActive { get; set; } = true;

        // Navigation property for employees
        public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public string? TeamLeaderId { get; set; }
        public string? DirectorId { get; set; }
    }
} 