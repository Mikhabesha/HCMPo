using System;
using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models
{
    public class SyncLog
    {
        public int Id { get; set; }
        
        [Required]
        public DateTime SyncTime { get; set; }
        
        [Required]
        public int RecordsSynced { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; }
        
        [StringLength(500)]
        public string ErrorMessage { get; set; }
    }
} 