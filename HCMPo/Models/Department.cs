using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models
{
    public class Department
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation property
        public ICollection<Employee>? Employees { get; set; }
    }
} 