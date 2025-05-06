using System;
using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models
{
    public class Notification
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string Title { get; set; }
        
        [Required]
        public string Message { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsRead { get; set; }
        
        public DateTime? ReadAt { get; set; }
        
        public string? Link { get; set; }
        
        public NotificationType Type { get; set; }
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
} 