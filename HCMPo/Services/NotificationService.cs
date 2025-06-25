using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using HCMPo.Models;
using HCMPo.Data;

namespace HCMPo.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task CreateNotificationAsync(string userId, string title, string message, string url = null, string type = "Info")
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                Url = url ?? "#"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await SendRealTimeNotification(userId, message);
        }

        public async Task MarkAsReadAsync(string notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var summary = await GetUnreadNotificationSummaryAsync(notification.UserId);
                await _hubContext.Clients.User(notification.UserId)
                    .SendAsync("UpdateBadgeCount", summary.count, summary.mostRecentType);
            }
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false, int limit = 50)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<(int count, string mostRecentType)> GetUnreadNotificationSummaryAsync(string userId)
        {
            var unreadNotifications = _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead);

            var count = await unreadNotifications.CountAsync();
            var mostRecentNotification = await unreadNotifications
                .OrderByDescending(n => n.CreatedAt)
                .FirstOrDefaultAsync();

            return (count, mostRecentNotification?.Type ?? "Info");
        }

        public async Task SendRealTimeNotification(string userId, string message)
        {
            await _hubContext.Clients.User(userId)
                .SendAsync("ReceiveNotification", message);

            var summary = await GetUnreadNotificationSummaryAsync(userId);
            await _hubContext.Clients.User(userId)
                .SendAsync("UpdateBadgeCount", summary.count, summary.mostRecentType);
        }

        // Backward compatibility methods
        public async Task CreateNotification(string userId, string title, string message, string type)
        {
            await CreateNotificationAsync(userId, title, message, "#", type);
        }

        public async Task MarkAsRead(string notificationId)
        {
            await MarkAsReadAsync(notificationId);
        }

        public async Task<List<Notification>> GetUserNotifications(string userId, bool unreadOnly = false)
        {
            return await GetUserNotificationsAsync(userId, unreadOnly, 50);
        }

        public async Task<int> GetUnreadCount(string userId)
        {
            return await GetUnreadCountAsync(userId);
        }
    }

    public class NotificationHub : Hub
    {
        public async Task JoinGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        public async Task LeaveGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }
    }
} 