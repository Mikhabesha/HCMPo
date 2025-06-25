using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HCMPo.Models;

namespace HCMPo.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string title, string message, string url = null, string type = "Info");
        Task MarkAsReadAsync(string notificationId);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false, int limit = 50);
        Task<int> GetUnreadCountAsync(string userId);
        Task<(int count, string mostRecentType)> GetUnreadNotificationSummaryAsync(string userId);
        Task SendRealTimeNotification(string userId, string message);
        
        // Backward compatibility methods
        Task CreateNotification(string userId, string title, string message, string type);
        Task MarkAsRead(string notificationId);
        Task<List<Notification>> GetUserNotifications(string userId, bool unreadOnly = false);
        Task<int> GetUnreadCount(string userId);
    }
} 