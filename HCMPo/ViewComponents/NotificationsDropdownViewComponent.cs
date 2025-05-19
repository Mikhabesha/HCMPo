using Microsoft.AspNetCore.Mvc;
using HCMPo.Models;
using Microsoft.EntityFrameworkCore;
using HCMPo.Data;

namespace HCMPo.ViewComponents
{
    public class NotificationsDropdownViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public NotificationsDropdownViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (currentUser == null)
            {
                return View("Default", new List<Notification>());
            }

            var notifications = await _context.Notifications
                .Where(n => n.UserId == currentUser.Id && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View("Default", notifications);
        }
    }
} 