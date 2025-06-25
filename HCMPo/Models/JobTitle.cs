using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public class JobTitle
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public string? Description { get; set; }

        public string? Grade { get; set; }

        public bool IsActive { get; set; } = true;

        [Display(Name = "Organization Unit")]
        public string? OrganizationUnitId { get; set; }

        [ForeignKey("OrganizationUnitId")]
        public virtual OrganizationUnit? OrganizationUnit { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
        
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
} 