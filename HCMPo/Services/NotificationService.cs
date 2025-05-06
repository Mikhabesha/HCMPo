using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using HCMPo.Models;

namespace HCMPo.Services
{
    public interface INotificationService
    {
        Task CreateNotification(string userId, string title, string message, NotificationType type);
        Task MarkAsRead(string notificationId);
        Task<List<Notification>> GetUserNotifications(string userId, bool unreadOnly = false);
        Task<int> GetUnreadCount(string userId);
        Task SendRealTimeNotification(string userId, string message);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task CreateNotification(string userId, string title, string message, NotificationType type)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification
            await SendRealTimeNotification(userId, message);
        }

        public async Task MarkAsRead(string notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Update badge count
                var unreadCount = await GetUnreadCount(notification.UserId);
                await _hubContext.Clients.User(notification.UserId)
                    .SendAsync("UpdateBadgeCount", unreadCount);
            }
        }

        public async Task<List<Notification>> GetUserNotifications(string userId, bool unreadOnly = false)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCount(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task SendRealTimeNotification(string userId, string message)
        {
            await _hubContext.Clients.User(userId)
                .SendAsync("ReceiveNotification", message);

            var unreadCount = await GetUnreadCount(userId);
            await _hubContext.Clients.User(userId)
                .SendAsync("UpdateBadgeCount", unreadCount);
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