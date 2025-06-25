using HCMPo.Models;
using System.Collections.Generic;

namespace HCMPo.ViewModels
{
    public class NotificationsDropdownViewModel
    {
        public List<Notification> Notifications { get; set; } = new List<Notification>();
        public int UnreadCount { get; set; }
    }
} 