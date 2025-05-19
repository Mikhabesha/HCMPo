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
            ViewBag.DepartmentCount = await _context.Departments.CountAsync();
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
