using HCMPo.Models;
using HCMPo.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HCMPo.Controllers
{
    [Authorize]
    public class LeaveRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LeaveRequestsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: LeaveRequests
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (User.IsInRole("Admin") || User.IsInRole("HR"))
            {
                // Admins and HR can see all leave requests
                var leaveRequests = await _context.LeaveRequests
                    .Include(l => l.Employee)
                    .Include(l => l.LeaveType)
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();
                return View(leaveRequests);
            }
            else
            {
                // Regular employees can only see their own leave requests
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.UserId == user.Id);

                if (employee == null)
                {
                    return NotFound("Employee profile not found. Please contact HR.");
                }

                var leaveRequests = await _context.LeaveRequests
                    .Include(l => l.Employee)
                    .Include(l => l.LeaveType)
                    .Where(l => l.EmployeeId == employee.Id)
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();
                return View(leaveRequests);
            }
        }

        // GET: LeaveRequests/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveRequest = await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (leaveRequest == null)
            {
                return NotFound();
            }

            return View(leaveRequest);
        }

        // GET: LeaveRequests/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            if (employee == null)
            {
                return NotFound("Employee profile not found. Please contact HR.");
            }

            ViewData["EmployeeId"] = employee.Id;
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name");
            return View();
        }

        // POST: LeaveRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeId,LeaveTypeId,StartDate,EndDate,Reason")] LeaveRequest leaveRequest)
        {
            if (ModelState.IsValid)
            {
                leaveRequest.Id = Guid.NewGuid().ToString();
                leaveRequest.Status = LeaveRequestStatus.Submitted;
                leaveRequest.CreatedAt = DateTime.UtcNow;

                // Calculate total days
                var totalDays = (leaveRequest.EndDate - leaveRequest.StartDate).TotalDays;
                if (leaveRequest.IsHalfDay)
                {
                    totalDays = 0.5;
                }
                leaveRequest.TotalDays = (decimal)totalDays;

                _context.Add(leaveRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["EmployeeId"] = leaveRequest.EmployeeId;
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name", leaveRequest.LeaveTypeId);
            return View(leaveRequest);
        }

        // GET: LeaveRequests/Approve/5
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Approve(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var currentEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == currentUser.Id);

            if (User.IsInRole("HR"))
            {
                leaveRequest.Status = LeaveRequestStatus.HRApproved;
                leaveRequest.HRId = currentEmployee.Id;
                leaveRequest.HRApprovalDate = DateTime.UtcNow;
            }
            else if (User.IsInRole("Admin"))
            {
                leaveRequest.Status = LeaveRequestStatus.DirectorApproved;
                leaveRequest.DirectorId = currentEmployee.Id;
                leaveRequest.DirectorApprovalDate = DateTime.UtcNow;
            }

            _context.Update(leaveRequest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: LeaveRequests/Reject/5
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Reject(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var currentEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == currentUser.Id);

            if (User.IsInRole("HR"))
            {
                leaveRequest.Status = LeaveRequestStatus.HRRejected;
                leaveRequest.HRId = currentEmployee.Id;
                leaveRequest.HRApprovalDate = DateTime.UtcNow;
            }
            else if (User.IsInRole("Admin"))
            {
                leaveRequest.Status = LeaveRequestStatus.DirectorRejected;
                leaveRequest.DirectorId = currentEmployee.Id;
                leaveRequest.DirectorApprovalDate = DateTime.UtcNow;
            }

            _context.Update(leaveRequest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
} 