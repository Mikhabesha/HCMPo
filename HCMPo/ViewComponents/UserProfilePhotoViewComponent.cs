using HCMPo.Data;
using HCMPo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HCMPo.ViewComponents
{
    public class UserProfilePhotoViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserProfilePhotoViewComponent(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string a_class = "rounded-circle", int size = 32)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            string photoUrl = null;

            if (user != null)
            {
                var employee = await _context.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.ApplicationUserId == user.Id);
                
                if (employee != null && !string.IsNullOrEmpty(employee.PhotoUrl))
                {
                    photoUrl = employee.PhotoUrl;
                }
            }
            
            ViewBag.CssClass = a_class;
            ViewBag.Size = size;
            return View("Default", photoUrl);
        }
    }
} 