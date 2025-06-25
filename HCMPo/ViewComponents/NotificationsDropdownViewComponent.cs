using Microsoft.AspNetCore.Mvc;
using HCMPo.Models;
using Microsoft.EntityFrameworkCore;
using HCMPo.Data;
using System.Security.Claims;
using HCMPo.Services;
using HCMPo.ViewModels;

namespace HCMPo.ViewComponents
{
    public class NotificationsDropdownViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public NotificationsDropdownViewComponent(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Content("");
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, true, 5);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            
            var model = new NotificationsDropdownViewModel
            {
                Notifications = notifications,
                UnreadCount = unreadCount
            };

            return View(model);
        }
    }
} 