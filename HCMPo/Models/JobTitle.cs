using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models
{
    public class JobTitle
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
} 