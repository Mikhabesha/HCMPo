using System;
using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models
{
    public class Holiday
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Name { get; set; }
    }
} 