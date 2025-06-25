using System.Diagnostics;
using HCMPo.Data;
using HCMPo.Models;
using HCMPo.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HCMPo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // For dashboard statistics
            ViewBag.EmployeeCount = await _context.Employees.CountAsync();
            ViewBag.DepartmentCount = await _context.OrganizationUnits.CountAsync(ou => ou.Type == OrganizationUnitType.Department);
            ViewBag.PendingLeaveRequests = await _context.LeaveRequests
                .Where(lr => lr.Status == LeaveRequestStatus.Submitted)
                .CountAsync();

            if (User.Identity.IsAuthenticated)
            {
                // Get pending leave requests for admins/HR
                if (User.IsInRole("Admin") || User.IsInRole("HR"))
                {
                    var pendingLeaves = await _context.LeaveRequests
                        .Where(l => l.Status == LeaveRequestStatus.Submitted)
                        .CountAsync();
                    ViewBag.PendingLeaves = pendingLeaves;
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Troubleshooting endpoint for "Request Too Long" errors
        [HttpGet]
        public IActionResult ClearSession()
        {
            try
            {
                // Clear session data
                HttpContext.Session.Clear();
                
                // Clear authentication cookies
                foreach (var cookie in Request.Cookies.Keys)
                {
                    if (cookie.Contains("Identity") || cookie.Contains("Auth") || cookie.Contains("Session"))
                    {
                        Response.Cookies.Delete(cookie);
                    }
                }
                
                TempData["SuccessMessage"] = "Session and cookies cleared successfully. Please log in again.";
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error clearing session: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
