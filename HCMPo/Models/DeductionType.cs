using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models
{
    public class DeductionType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int Order { get; set; } = 0;
    }
} 