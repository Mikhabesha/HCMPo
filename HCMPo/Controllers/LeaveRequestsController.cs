using HCMPo.Data;
using HCMPo.Helpers;
using HCMPo.Models;
using HCMPo.Models.Enums;
using HCMPo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;

namespace HCMPo.Controllers
{
    [Authorize]
    [Route("Leave")]
    [Route("LeaveRequests")]
    public class LeaveRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILeaveService _leaveService;
        private readonly INotificationService _notificationService;

        public LeaveRequestsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILeaveService leaveService,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _leaveService = leaveService;
            _notificationService = notificationService;
        }

        // GET: LeaveRequests
        [HttpGet("")]
        [HttpGet("Index")]
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
                var employee = user.EmployeeId != null 
                    ? await _context.Employees.FirstOrDefaultAsync(e => e.Id == user.EmployeeId)
                    : null;

                if (employee == null)
                {
                    return NotFound("Employee profile not found. Please contact HR.");
                }

                var leaveRequests = await _leaveService.GetEmployeeLeaveRequestsAsync(employee.Id);
                return View(leaveRequests);
            }
        }

        // GET: LeaveRequests/Details/5
        [HttpGet("Details/{id}")]
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
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            // Find employee using ApplicationUser.EmployeeId relationship
            var employee = user.EmployeeId != null 
                ? await _context.Employees
                    .Include(e => e.OrganizationUnit)
                    .Include(e => e.JobTitle)
                    .FirstOrDefaultAsync(e => e.Id == user.EmployeeId)
                : null;

            if (employee == null)
            {
                // Try with email
                employee = await _context.Employees
                    .Include(e => e.OrganizationUnit)
                    .Include(e => e.JobTitle)
                    .FirstOrDefaultAsync(e => e.Email == user.Email);
            }

            if (employee == null)
            {
                // Create detailed diagnostic information
                var debugInfo = new {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    UserName = user.UserName,
                    EmployeeCount = await _context.Employees.CountAsync(),
                    SampleEmployees = await _context.Employees
                        .Take(5)
                        .Select(e => new { e.Id, e.FirstName, e.LastName, e.Email, e.UserId, e.ApplicationUserId })
                        .ToListAsync()
                };

                TempData["ErrorMessage"] = $"Employee profile not found. Please contact HR to link your account. " +
                    $"User ID: {user.Id}, Email: {user.Email}";
                TempData["DebugInfo"] = JsonSerializer.Serialize(debugInfo);
                
                return RedirectToAction("Index", "Home");
            }

            // Update employee record with correct user reference if needed
            if (string.IsNullOrEmpty(employee.ApplicationUserId) && !string.IsNullOrEmpty(user.Id))
            {
                employee.ApplicationUserId = user.Id;
                employee.UserId = user.Id; // Also set UserId for backward compatibility
                await _context.SaveChangesAsync();
            }

            ViewData["EmployeeId"] = employee.Id;
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes.Where(lt => lt.IsActive), "Id", "Name");
            
            // Get leave balances for the current year
            var year = DateTime.Now.Year;
            var leaveBalances = await _leaveService.GetEmployeeLeaveBalancesAsync(employee.Id, year);
            ViewBag.LeaveBalances = leaveBalances;
            ViewBag.Employee = employee;
            
            // Create a model with EmployeeId pre-populated
            var model = new LeaveRequest
            {
                EmployeeId = employee.Id
            };
            
            return View(model);
        }

        // POST: LeaveRequests/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeId,LeaveTypeId,StartDate,EndDate,Reason,IsHalfDay")] LeaveRequest leaveRequest)
        {
            Console.WriteLine("=== CREATE LEAVE REQUEST START ===");
            Console.WriteLine($"Received LeaveRequest: EmployeeId={leaveRequest.EmployeeId}, LeaveTypeId={leaveRequest.LeaveTypeId}");
            Console.WriteLine($"Dates: Start={leaveRequest.StartDate}, End={leaveRequest.EndDate}, IsHalfDay={leaveRequest.IsHalfDay}");
            Console.WriteLine($"Reason: {leaveRequest.Reason}");
            
            try
            {
                // Get the current user and their employee record
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    Console.WriteLine("❌ User not found");
                    TempData["ErrorMessage"] = "User not found. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                Console.WriteLine($"Current user: {user.Email} (ID: {user.Id})");

                // Use ApplicationUser.EmployeeId lookup
                var employee = user.EmployeeId != null 
                    ? await _context.Employees.FirstOrDefaultAsync(e => e.Id == user.EmployeeId)
                    : null;

                if (employee == null)
                {
                    Console.WriteLine("⚠️ Employee not found by EmployeeId, trying Email...");
                    employee = await _context.Employees
                        .FirstOrDefaultAsync(e => e.Email == user.Email);
                }

                if (employee == null)
                {
                    Console.WriteLine("❌ Employee profile not found after all attempts");
                    TempData["ErrorMessage"] = "Employee profile not found. Please contact HR to link your account.";
                    return RedirectToAction("Index", "Home");
                }

                Console.WriteLine($"✅ Employee found: {employee.FullName} (ID: {employee.Id})");

                // Security: Ensure the EmployeeId is set to the current user's employee ID
                leaveRequest.EmployeeId = employee.Id;
                Console.WriteLine($"Set EmployeeId to: {employee.Id}");

                // Basic validation with improved error messages
                var validationErrors = new List<string>();

                if (string.IsNullOrEmpty(leaveRequest.LeaveTypeId))
                {
                    validationErrors.Add("Please select a leave type.");
                    Console.WriteLine("❌ Leave type not selected");
                }

                if (leaveRequest.StartDate == default)
                {
                    validationErrors.Add("Please select a start date.");
                    Console.WriteLine("❌ Start date not set");
                }

                if (!leaveRequest.IsHalfDay && leaveRequest.EndDate == default)
                {
                    validationErrors.Add("Please select an end date.");
                    Console.WriteLine("❌ End date not set for non-half-day request");
                }

                // Allow same-day or future date requests (remove past date restriction for flexibility)
                if (leaveRequest.StartDate < DateTime.Today.AddDays(-1))
                {
                    validationErrors.Add("Start date cannot be more than one day in the past.");
                    Console.WriteLine("❌ Start date too far in the past");
                }

                if (!leaveRequest.IsHalfDay && leaveRequest.StartDate > leaveRequest.EndDate)
                {
                    validationErrors.Add("End date must be on or after start date.");
                    Console.WriteLine("❌ End date before start date");
                }

                if (string.IsNullOrWhiteSpace(leaveRequest.Reason) || leaveRequest.Reason.Length < 5)
                {
                    validationErrors.Add("Please provide a reason (minimum 5 characters).");
                    Console.WriteLine("❌ Reason too short or empty");
                }

                // If half day, set end date to start date
                if (leaveRequest.IsHalfDay)
                {
                    leaveRequest.EndDate = leaveRequest.StartDate;
                    Console.WriteLine($"✅ Half day request - set end date to start date: {leaveRequest.EndDate}");
                }

                // Calculate working days
                var workingDays = await _leaveService.CalculateWorkingDaysAsync(leaveRequest.StartDate, leaveRequest.EndDate);
                leaveRequest.TotalDays = leaveRequest.IsHalfDay ? 0.5m : workingDays;
                Console.WriteLine($"✅ Calculated working days: {workingDays}, Total days: {leaveRequest.TotalDays}");

                // Check leave balance (warning, not blocking)
                var year = leaveRequest.StartDate.Year;
                var balance = await _leaveService.GetLeaveBalanceAsync(leaveRequest.EmployeeId, leaveRequest.LeaveTypeId, year);
                if (balance != null && leaveRequest.TotalDays > balance.RemainingDays)
                {
                    validationErrors.Add($"Insufficient leave balance. Available: {balance.RemainingDays} days, Requested: {leaveRequest.TotalDays} days.");
                    Console.WriteLine($"⚠️ Insufficient balance - Available: {balance.RemainingDays}, Requested: {leaveRequest.TotalDays}");
                }

                // Check for overlapping leave (warning, not blocking for HR flexibility)
                var hasOverlap = await _leaveService.HasOverlappingLeaveAsync(leaveRequest.EmployeeId, leaveRequest.StartDate, leaveRequest.EndDate);
                if (hasOverlap)
                {
                    validationErrors.Add("Warning: You have overlapping leave requests. Please verify your dates.");
                    Console.WriteLine("⚠️ Overlapping leave detected");
                }

                Console.WriteLine($"Validation errors count: {validationErrors.Count}");
                foreach (var error in validationErrors)
                {
                    Console.WriteLine($"  - {error}");
                }

                // Add validation errors to ModelState
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError("", error);
                }

                // Allow submission even with warnings for HR to review
                if (validationErrors.Count <= 2) // Allow if only minor issues
                {
                    Console.WriteLine("✅ Attempting to create leave request...");
                    try
                    {
                        var createdRequest = await _leaveService.CreateLeaveRequestAsync(leaveRequest);
                        Console.WriteLine($"✅ Leave request created successfully with ID: {createdRequest.Id}");
                        TempData["SuccessMessage"] = $"Leave request submitted successfully! Request ID: {createdRequest.Id}";
                return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error creating leave request: {ex}");
                        ModelState.AddModelError("", $"Error creating leave request: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ Too many validation errors ({validationErrors.Count}) - not submitting");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Outer exception in Create: {ex}");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            Console.WriteLine("=== CREATE LEAVE REQUEST END (RETURNING TO FORM) ===");
            
            // If we got this far, something failed, redisplay form
            try
            {
                ViewData["LeaveTypeId"] = new SelectList(
                    await _context.LeaveTypes.Where(lt => lt.IsActive).ToListAsync(), 
                    "Id", "Name", leaveRequest.LeaveTypeId);
                
                // Re-populate leave balances and employee info
                if (!string.IsNullOrEmpty(leaveRequest.EmployeeId))
                {
                    var year = DateTime.Now.Year;
                    var leaveBalances = await _leaveService.GetEmployeeLeaveBalancesAsync(leaveRequest.EmployeeId, year);
                    var employee = await _context.Employees.FindAsync(leaveRequest.EmployeeId);
                    
                    ViewBag.LeaveBalances = leaveBalances;
                    ViewBag.Employee = employee;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error populating ViewBag data: {ex}");
                ViewData["LeaveTypeId"] = new SelectList(new List<LeaveType>(), "Id", "Name");
                ViewBag.LeaveBalances = new List<EmployeeLeave>();
            }
            
            return View(leaveRequest);
        }

        // POST: Process Leave Approval (Multi-level workflow)
        [HttpPost("ProcessApproval")]
        [Authorize(Roles = "Admin,HR,TeamLeader,Director")]
        public async Task<IActionResult> ProcessApproval(string requestId, bool approve, string remarks = null)
        {
            if (string.IsNullOrEmpty(requestId))
            {
                return Json(new { success = false, message = "Request ID is required" });
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var currentEmployee = currentUser.EmployeeId != null 
                    ? await _context.Employees.FirstOrDefaultAsync(e => e.Id == currentUser.EmployeeId)
                    : null;

                if (currentEmployee == null)
                {
                    return Json(new { success = false, message = "Employee profile not found" });
                }

                // Check if user can approve this request
                var canApprove = await _leaveService.CanUserApproveRequestAsync(requestId, currentUser.Id);
                if (!canApprove)
                {
                    return Json(new { success = false, message = "You are not authorized to approve this request" });
                }

                var processedRequest = await _leaveService.ProcessLeaveApprovalAsync(requestId, currentEmployee.Id, approve, remarks);
                
                var nextApprover = await _leaveService.GetNextApproverAsync(processedRequest, currentUser);
                var actionText = approve ? "approved" : "rejected";
                var statusText = GetStatusDisplayText(processedRequest.Status);

                return Json(new { 
                    success = true, 
                    message = $"Leave request {actionText} successfully. Status: {statusText}",
                    status = processedRequest.Status.ToString(),
                    nextApprover = nextApprover,
                    isComplete = processedRequest.Status == LeaveRequestStatus.HRApproved || 
                                processedRequest.Status.ToString().Contains("Rejected")
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing approval: {ex}");
                return Json(new { success = false, message = "An error occurred while processing the approval" });
            }
        }

        // GET: My Pending Approvals
        [HttpGet("MyApprovals")]
        [Authorize(Roles = "Admin,HR,TeamLeader,Director")]
        public async Task<IActionResult> MyApprovals()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var pendingApprovals = await _leaveService.GetPendingApprovalsForUserAsync(currentUser.Id);

            ViewBag.UserRoles = await _userManager.GetRolesAsync(currentUser);
            return View(pendingApprovals);
        }

        // POST: Approve Leave Request
        [HttpPost("Approve/{id}")]
        [Authorize(Roles = "Admin,HR,TeamLeader,Director")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id, string remarks)
        {
            var result = await ProcessLegacyApproval(id, true, remarks);
            var leaveRequest = await _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType)
                .FirstOrDefaultAsync(lr => lr.Id == id);

            if (leaveRequest != null && leaveRequest.Employee != null)
            {
                await _notificationService.CreateNotificationAsync(
                    leaveRequest.Employee.ApplicationUserId,
                    "Leave Request Approved",
                    $"Your leave request for {leaveRequest.LeaveType.Name} from {leaveRequest.StartDate:d} to {leaveRequest.EndDate:d} has been approved.",
                    Url.Action("Details", new { id = leaveRequest.Id }),
                    "leave");
            }
            return result;
        }

        // POST: Reject Leave Request
        [HttpPost("Reject/{id}")]
        [Authorize(Roles = "Admin,HR,TeamLeader,Director")]  
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id, string remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks))
            {
                TempData["ErrorMessage"] = "Rejection remarks are required.";
                return RedirectToAction(nameof(Details), new { id });
            }
            var result = await ProcessLegacyApproval(id, false, remarks);
            var leaveRequest = await _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType)
                .FirstOrDefaultAsync(lr => lr.Id == id);
            if (leaveRequest != null && leaveRequest.Employee != null)
            {
                await _notificationService.CreateNotificationAsync(
                    leaveRequest.Employee.ApplicationUserId,
                    "Leave Request Rejected",
                    $"Your leave request for {leaveRequest.LeaveType.Name} from {leaveRequest.StartDate:d} to {leaveRequest.EndDate:d} has been rejected. Reason: {remarks}",
                    Url.Action("Details", new { id = leaveRequest.Id }),
                    "leave");
            }
            return result;
        }

        private async Task<IActionResult> ProcessLegacyApproval(string id, bool approve, string remarks)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Request ID is required.";
                return RedirectToAction(nameof(Index));
            }

            if (!approve && string.IsNullOrWhiteSpace(remarks))
            {
                TempData["ErrorMessage"] = "Rejection reason is required.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "Could not identify the current user. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }
                
                var currentEmployee = currentUser.EmployeeId != null 
                    ? await _context.Employees.FirstOrDefaultAsync(e => e.Id == currentUser.EmployeeId)
                    : null;

                if (currentEmployee == null)
                {
                    TempData["ErrorMessage"] = "Employee profile not found.";
                    return RedirectToAction(nameof(Index));
                }

                var canApprove = await _leaveService.CanUserApproveRequestAsync(id, currentUser.Id);
                if (!canApprove)
                {
                    TempData["ErrorMessage"] = "You are not authorized to approve this request.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var processedRequest = await _leaveService.ProcessLeaveApprovalAsync(id, currentEmployee.Id, approve, remarks);
                
                var actionText = approve ? "approved" : "rejected";
                TempData["SuccessMessage"] = $"Leave request {actionText} successfully!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in legacy approval: {ex}");
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private string GetStatusDisplayText(LeaveRequestStatus status)
        {
            return status switch
            {
                LeaveRequestStatus.Submitted => "Pending Team Leader Approval",
                LeaveRequestStatus.TeamLeaderApproved => "Pending Director Approval",
                LeaveRequestStatus.DirectorApproved => "Pending HR Final Approval", 
                LeaveRequestStatus.HRApproved => "Fully Approved & Processed",
                LeaveRequestStatus.TeamLeaderRejected => "Rejected by Team Leader",
                LeaveRequestStatus.DirectorRejected => "Rejected by Director",
                LeaveRequestStatus.HRRejected => "Rejected by HR",
                _ => status.ToString()
            };
        }

        // GET: Convert Ethiopian date to Gregorian
        [HttpGet("ConvertEthiopianDate")]
        public IActionResult ConvertEthiopianDate(int year, int month, int day)
        {
            try
            {
                var gregorianDate = EthiopianCalendarHelper.ToGregorianDate(year, month, day);
                return Json(new { 
                    success = true, 
                    gregorianDate = gregorianDate.ToString("yyyy-MM-dd"),
                    formatted = gregorianDate.ToString("MMM dd, yyyy")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // GET: API endpoint for leave balance
        [HttpGet("GetLeaveBalance")]
        public async Task<IActionResult> GetLeaveBalance(string employeeId, string leaveTypeId, int year = 0)
        {
            try
            {
                if (year == 0) year = DateTime.Now.Year;
                
                // Enhanced debugging
                var currentUser = await _userManager.GetUserAsync(User);
                Console.WriteLine($"=== GetLeaveBalance Debug ===");
                Console.WriteLine($"Current User: {currentUser?.Email} (ID: {currentUser?.Id})");
                Console.WriteLine($"Requested EmployeeId: {employeeId}");
                Console.WriteLine($"LeaveTypeId: {leaveTypeId}");
                Console.WriteLine($"Year: {year}");
                
                // Validate input parameters
                if (string.IsNullOrEmpty(employeeId))
                {
                    Console.WriteLine("❌ Employee ID is empty");
                    return Json(new { success = false, message = "Employee ID is required", debug = "employeeId parameter is empty" });
                }
                
                if (string.IsNullOrEmpty(leaveTypeId))
                {
                    Console.WriteLine("❌ Leave Type ID is empty");
                    return Json(new { success = false, message = "Leave Type ID is required", debug = "leaveTypeId parameter is empty" });
                }
                
                // Check if employee exists
                var employee = await _context.Employees.FindAsync(employeeId);
                Console.WriteLine($"Employee lookup result: {(employee != null ? $"Found - {employee.FullName}" : "Not found")}");
                
                if (employee == null)
                {
                    // Enhanced employee search for debugging
                    var allEmployees = await _context.Employees
                        .Take(5)
                        .Select(e => new { e.Id, e.FullName, e.Email, e.UserId, e.ApplicationUserId })
                        .ToListAsync();
                    
                    Console.WriteLine($"❌ Employee not found. Sample employees: {System.Text.Json.JsonSerializer.Serialize(allEmployees)}");
                    
                    return Json(new { 
                        success = false, 
                        message = "Employee not found", 
                        debug = new {
                            searchedEmployeeId = employeeId,
                            currentUserId = currentUser?.Id,
                            currentUserEmail = currentUser?.Email,
                            sampleEmployees = allEmployees,
                            hint = "Visit /Leave/Debug to see employee lookup details"
                        }
                    });
                }
                
                // Check if leave type exists
                var leaveType = await _context.LeaveTypes.FindAsync(leaveTypeId);
                Console.WriteLine($"Leave type lookup result: {(leaveType != null ? $"Found - {leaveType.Name}" : "Not found")}");
                
                if (leaveType == null)
                {
                    Console.WriteLine("❌ Leave type not found");
                    return Json(new { success = false, message = "Leave type not found" });
                }
                
                var balance = await _leaveService.GetLeaveBalanceAsync(employeeId, leaveTypeId, year);
                Console.WriteLine($"Leave balance result: {(balance != null ? $"Found - Total: {balance.TotalDays}, Used: {balance.UsedDays}, Remaining: {balance.RemainingDays}" : "Not found")}");
                
                if (balance == null)
                {
                    Console.WriteLine("⚠️ Balance not found, attempting to initialize...");
                    // Try to initialize balances and get again
                    await _leaveService.InitializeEmployeeLeaveBalancesAsync(employeeId, year);
                    balance = await _leaveService.GetLeaveBalanceAsync(employeeId, leaveTypeId, year);
                    
                    if (balance == null)
                    {
                        Console.WriteLine("❌ Unable to create leave balance after initialization");
                        return Json(new { success = false, message = "Unable to create leave balance. Please contact HR." });
                    }
                    else
                    {
                        Console.WriteLine("✅ Leave balance created successfully after initialization");
                    }
                }

                Console.WriteLine("✅ Returning successful leave balance");
                return Json(new 
                { 
                    success = true, 
                    totalDays = balance.TotalDays,
                    usedDays = balance.UsedDays,
                    remainingDays = balance.RemainingDays,
                    leaveTypeName = balance.LeaveType?.Name ?? leaveType.Name,
                    year = year,
                    debug = new {
                        employeeName = employee.FullName,
                        leaveTypeName = leaveType.Name
                    }
                });
            }
            catch (Exception ex)
            {
                // Log the full error for debugging
                Console.WriteLine($"❌ GetLeaveBalance Error: {ex}");
                return Json(new { success = false, message = $"Server error: {ex.Message}", debug = ex.ToString() });
            }
        }

        // GET: Check working days
        [HttpGet("CalculateWorkingDays")]
        public async Task<IActionResult> CalculateWorkingDays(DateTime startDate, DateTime endDate, bool isHalfDay = false)
        {
            try
            {
                if (startDate == default || endDate == default)
                {
                    return Json(new { success = false, message = "Invalid dates provided" });
                }

                if (startDate > endDate)
                {
                    return Json(new { success = false, message = "Start date cannot be after end date" });
                }

                var workingDays = await _leaveService.CalculateWorkingDaysAsync(startDate, endDate);
                var totalDays = isHalfDay ? 0.5 : (endDate - startDate).Days + 1;
                var weekendDays = totalDays - workingDays;

                return Json(new { 
                    success = true,
                    workingDays = isHalfDay ? 0.5 : workingDays,
                    totalDays = isHalfDay ? 0.5 : totalDays,
                    weekendDays = isHalfDay ? 0 : weekendDays,
                    isHalfDay = isHalfDay
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error calculating working days: {ex.Message}" });
            }
        }

        // GET: Leave Calendar
        [HttpGet("Calendar")]
        public async Task<IActionResult> Calendar(int? ethiopianYear = null)
        {
            // Get current Ethiopian year if not specified
            if (!ethiopianYear.HasValue)
            {
                var currentEthiopianDate = EthiopianCalendarHelper.GetCurrentEthiopianDate();
                ethiopianYear = currentEthiopianDate.Year;
            }

            // Convert Ethiopian year to Gregorian year range
            // Ethiopian year spans across two Gregorian years
            var gregorianYearStart = ethiopianYear.Value + 7; // Ethiopian year starts in September of previous Gregorian year
            var gregorianYearEnd = gregorianYearStart + 1;

            // Get leave requests for the Ethiopian year (spanning two Gregorian years)
            var leaveRequests = await _context.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .Where(r => r.Status == LeaveRequestStatus.HRApproved && 
                           (r.StartDate.Year == gregorianYearStart || r.StartDate.Year == gregorianYearEnd))
                .ToListAsync();

            // Add Ethiopian date information to each leave request
            var leaveRequestsWithEthiopianDates = leaveRequests.Select(lr => new
            {
                lr.Id,
                lr.Employee,
                lr.LeaveType,
                lr.StartDate,
                lr.EndDate,
                lr.Status,
                lr.Reason,
                lr.TotalDays,
                StartDateEthiopian = EthiopianCalendarHelper.FromGregorianDate(lr.StartDate),
                EndDateEthiopian = EthiopianCalendarHelper.FromGregorianDate(lr.EndDate),
                StartDateEthiopianFormatted = EthiopianCalendarHelper.FormatEthiopianShort(lr.StartDate),
                EndDateEthiopianFormatted = EthiopianCalendarHelper.FormatEthiopianShort(lr.EndDate)
            }).ToList();

            ViewBag.EthiopianYear = ethiopianYear.Value;
            ViewBag.GregorianYearStart = gregorianYearStart;
            ViewBag.GregorianYearEnd = gregorianYearEnd;
            ViewBag.CurrentEthiopianDate = EthiopianCalendarHelper.GetCurrentEthiopianDate();
            ViewBag.EthiopianMonths = EthiopianCalendarHelper.AmharicMonths;

            return View(leaveRequestsWithEthiopianDates);
        }

        // GET: Leave Reports
        [HttpGet("Reports")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Reports()
        {
            var year = DateTime.Now.Year;
            
            var totalRequests = await _context.LeaveRequests
                .Where(r => r.StartDate.Year == year)
                .CountAsync();

            var approvedRequests = await _context.LeaveRequests
                .Where(r => r.StartDate.Year == year && r.Status == LeaveRequestStatus.HRApproved)
                .CountAsync();

            var pendingRequests = await _context.LeaveRequests
    .Where(r => r.StartDate.Year == year && 
               r.Status != LeaveRequestStatus.HRApproved && 
               r.Status != LeaveRequestStatus.HRRejected &&
               r.Status != LeaveRequestStatus.TeamLeaderRejected &&
               r.Status != LeaveRequestStatus.DirectorRejected)
    .CountAsync();

var rejectedRequests = await _context.LeaveRequests
    .Where(r => r.StartDate.Year == year && 
               (r.Status == LeaveRequestStatus.HRRejected ||
                r.Status == LeaveRequestStatus.TeamLeaderRejected ||
                r.Status == LeaveRequestStatus.DirectorRejected))
    .CountAsync();

            // Leave by type
            var leaveByType = await _context.LeaveRequests
                .Include(r => r.LeaveType)
                .Where(r => r.StartDate.Year == year && r.Status == LeaveRequestStatus.HRApproved)
                .GroupBy(r => r.LeaveType.Name)
                .Select(g => new { LeaveType = g.Key, Count = g.Count(), TotalDays = g.Sum(r => r.TotalDays) })
                .ToListAsync();

            ViewBag.TotalRequests = totalRequests;
            ViewBag.ApprovedRequests = approvedRequests;
            ViewBag.PendingRequests = pendingRequests;
            ViewBag.RejectedRequests = rejectedRequests;
            ViewBag.LeaveByType = leaveByType;

            return View();
        }

        // GET: My Leave Balance
        [HttpGet("MyBalance")]
        public async Task<IActionResult> MyBalance()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.EmployeeId == null)
            {
                TempData["ErrorMessage"] = "Your user account is not linked to an employee profile.";
                return RedirectToAction("Index", "Home");
            }

            var employee = await _context.Employees
                .Include(e => e.OrganizationUnit)
                .FirstOrDefaultAsync(e => e.Id == user.EmployeeId);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee profile not found.";
                return RedirectToAction("Index", "Home");
            }

            var year = DateTime.Now.Year;
            var balances = await _leaveService.GetEmployeeLeaveBalancesAsync(user.EmployeeId, year);

            ViewBag.Employee = employee;
            ViewBag.Year = year;

            return View(balances);
        }

        // GET: Diagnostic info for troubleshooting
        [HttpGet("Debug")]
        public async Task<IActionResult> Debug()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { error = "No user found" });
            }

            var employee = await _context.Employees
                .Include(e => e.OrganizationUnit)
                .Include(e => e.JobTitle)
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            var employeeByEmail = await _context.Employees
                .Include(e => e.OrganizationUnit)
                .Include(e => e.JobTitle)
                .FirstOrDefaultAsync(e => e.Email == user.Email);

            var allEmployees = await _context.Employees
                .Select(e => new { e.Id, FullName = e.FirstName + " " + e.LastName, e.Email, e.UserId })
                .Take(10)
                .ToListAsync();

            return Json(new {
                currentUser = new {
                    id = user.Id,
                    email = user.Email,
                    userName = user.UserName
                },
                employeeByUserId = employee == null ? null : new {
                    id = employee.Id,
                    name = employee.FullName,
                    email = employee.Email,
                    userId = employee.UserId,
                    department = employee.OrganizationUnit?.Name
                },
                employeeByEmail = employeeByEmail == null ? null : new {
                    id = employeeByEmail.Id,
                    name = employeeByEmail.FullName,
                    email = employeeByEmail.Email,
                    userId = employeeByEmail.UserId,
                    department = employeeByEmail.OrganizationUnit?.Name
                },
                sampleEmployees = allEmployees
            });
        }

        // GET: Debug Leave Requests - Check what's in the database
        [HttpGet("DebugRequests")]
        public async Task<IActionResult> DebugRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            
            // Get all leave requests (for debugging)
            var allRequests = await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Select(l => new {
                    l.Id,
                    l.EmployeeId,
                    EmployeeName = l.Employee != null ? l.Employee.FirstName + " " + l.Employee.LastName : "NULL",
                    l.LeaveTypeId,
                    LeaveTypeName = l.LeaveType != null ? l.LeaveType.Name : "NULL",
                    l.StartDate,
                    l.EndDate,
                    l.Status,
                    l.Reason,
                    l.CreatedAt
                })
                .OrderByDescending(l => l.CreatedAt)
                .Take(20)
                .ToListAsync();

            // Check current user's employee record
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            // Check user roles
            var userRoles = await _userManager.GetRolesAsync(user);

            // Check leave types
            var leaveTypes = await _context.LeaveTypes.ToListAsync();

            return Json(new {
                currentUser = new {
                    id = user.Id,
                    email = user.Email,
                    roles = userRoles
                },
                hasEmployeeProfile = employee != null,
                employeeId = employee?.Id,
                employeeName = employee?.FullName,
                totalRequestsInDb = allRequests.Count,
                allRequests = allRequests,
                leaveTypes = leaveTypes.Select(lt => new { 
                    lt.Id, 
                    lt.Name, 
                    lt.IsActive, 
                    lt.DefaultDays 
                }),
                leaveTypesCount = leaveTypes.Count,
                debugMessage = employee == null ? 
                    "❌ No employee profile found - You need to visit /Leave/CreateAdminProfile" : 
                    leaveTypes.Count == 0 ? 
                        "❌ No leave types found - Visit /Leave/InitializeLeaveTypes" :
                        "✅ Employee profile found - Leave requests should be visible"
            });
        }

        // GET: Initialize Leave Types (Quick Fix)
        [HttpGet("InitializeLeaveTypes")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> InitializeLeaveTypes()
        {
            var existingTypes = await _context.LeaveTypes.CountAsync();
            
            if (existingTypes > 0)
            {
                return Json(new { 
                    success = true, 
                    message = $"Leave types already exist ({existingTypes} types found)",
                    action = "no_action_needed"
                });
            }

            // Create default leave types
            var leaveTypes = new[]
            {
                new LeaveType
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Annual Leave",
                    Description = "Paid annual vacation leave",
                    DefaultDays = 20,
                    MaxDaysPerRequest = 15,
                    IsActive = true,
                    IsPaid = true,
                    RequiresApproval = true,
                    AllowHalfDay = true,
                    CreatedAt = DateTime.UtcNow
                },
                new LeaveType
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Sick Leave",
                    Description = "Paid sick leave",
                    DefaultDays = 10,
                    MaxDaysPerRequest = 5,
                    IsActive = true,
                    IsPaid = true,
                    RequiresApproval = true,
                    RequiresAttachment = true,
                    AllowHalfDay = true,
                    CreatedAt = DateTime.UtcNow
                },
                new LeaveType
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Personal Leave",
                    Description = "Personal time off",
                    DefaultDays = 5,
                    MaxDaysPerRequest = 3,
                    IsActive = true,
                    IsPaid = false,
                    RequiresApproval = true,
                    AllowHalfDay = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.LeaveTypes.AddRange(leaveTypes);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = $"Successfully created {leaveTypes.Length} leave types",
                leaveTypes = leaveTypes.Select(lt => new { lt.Name, lt.DefaultDays, lt.Description })
            });
        }

        // GET: Create Admin Profile (Quick Fix for Admin Users)
        [HttpGet("CreateAdminProfile")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAdminProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Check if employee profile already exists
            var existingEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == currentUser.Email);

            if (existingEmployee != null)
            {
                // Update the existing profile to link with current user
                existingEmployee.UserId = currentUser.Id;
                existingEmployee.ApplicationUserId = currentUser.Id;
                if (string.IsNullOrEmpty(existingEmployee.Email))
                {
                    existingEmployee.Email = currentUser.Email;
                }
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Linked existing employee profile: {existingEmployee.FullName}",
                    employeeId = existingEmployee.Id
                });
            }

            // Create new employee profile for admin
            var adminEmployee = new Employee
            {
                Id = Guid.NewGuid().ToString(),
                UserId = currentUser.Id,
                ApplicationUserId = currentUser.Id,
                FirstName = currentUser.UserName ?? "Admin",
                LastName = "User",
                Email = currentUser.Email,
                BadgeNumber = "ADMIN001",
                HireDate = DateTime.Now,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUser.Id
            };

            // Try to get HR department, or create one
            var hrDepartment = await _context.OrganizationUnits.FirstOrDefaultAsync(ou => ou.Type == OrganizationUnitType.Department && (ou.Name.Contains("HR") || ou.Name.Contains("Human")));
            if (hrDepartment == null)
            {
                hrDepartment = new OrganizationUnit
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Human Resources",
                    Description = "Human Resources Department",
                    Type = OrganizationUnitType.Department,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.OrganizationUnits.Add(hrDepartment);
            }

            adminEmployee.OrganizationUnitId = hrDepartment.Id;

            // Try to get HR job title, or create one
            var hrJobTitle = await _context.JobTitles.FirstOrDefaultAsync(j => j.Title.Contains("HR") || j.Title.Contains("Admin"));
            if (hrJobTitle == null)
            {
                hrJobTitle = new JobTitle
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "HR Administrator",
                    Description = "Human Resources Administrator",
                    Grade = "Senior",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.JobTitles.Add(hrJobTitle);
            }

            adminEmployee.JobTitleId = hrJobTitle.Id;

            _context.Employees.Add(adminEmployee);
            await _context.SaveChangesAsync();

            // Initialize leave balances
            try
            {
                await _leaveService.InitializeEmployeeLeaveBalancesAsync(adminEmployee.Id, DateTime.Now.Year);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not initialize leave balances: {ex.Message}");
            }

            return Json(new { 
                success = true, 
                message = $"Created admin employee profile: {adminEmployee.FullName}",
                employeeId = adminEmployee.Id,
                hint = "You can now create leave requests!"
            });
        }

        // GET: Link Existing Employees to Users (Admin Tool)
        [HttpGet("LinkEmployeesToUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LinkEmployeesToUsers()
        {
            var linkedCount = 0;
            var errors = new List<string>();

            // Get all employees without userId
            var employeesWithoutUsers = await _context.Employees
                .Where(e => e.UserId == null)
                .ToListAsync();

            // Get all users
            var allUsers = await _userManager.Users.ToListAsync();

            foreach (var employee in employeesWithoutUsers)
            {
                if (!string.IsNullOrEmpty(employee.Email))
                {
                    // Try to find user by email
                    var user = allUsers.FirstOrDefault(u => u.Email.Equals(employee.Email, StringComparison.OrdinalIgnoreCase));
                    
                    if (user != null)
                    {
                        employee.UserId = user.Id;
                        linkedCount++;
                    }
                    else
                    {
                        errors.Add($"No user found for employee: {employee.FullName} ({employee.Email})");
                    }
                }
                else
                {
                    errors.Add($"Employee {employee.FullName} has no email address");
                }
            }

            if (linkedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return Json(new {
                success = true,
                linkedCount,
                totalEmployees = employeesWithoutUsers.Count,
                errors,
                message = $"Successfully linked {linkedCount} employees to users"
            });
        }

        // GET: Leave Entitlement Management (HR Only)
        [HttpGet("ManageEntitlements")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> ManageEntitlements(string employeeId = null)
        {
            var employees = await _context.Employees
                .Include(e => e.OrganizationUnit)
                .Include(e => e.JobTitle)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync();

            ViewBag.Employees = new SelectList(employees, "Id", "FullName", employeeId);

            if (!string.IsNullOrEmpty(employeeId))
            {
                var entitlements = await _leaveService.GetAllEmployeeEntitlementsAsync(employeeId);
                var employee = employees.FirstOrDefault(e => e.Id == employeeId);
                var yearsOfService = await _leaveService.CalculateYearsOfServiceAsync(employeeId);
                
                ViewBag.SelectedEmployee = employee;
                ViewBag.YearsOfService = yearsOfService;
                ViewBag.Entitlements = entitlements;
                
                // Get current year balances
                var currentBalances = await _leaveService.GetEmployeeLeaveBalancesAsync(employeeId, DateTime.Now.Year);
                ViewBag.CurrentBalances = currentBalances;
            }

            var leaveTypes = await _context.LeaveTypes.Where(lt => lt.IsActive).ToListAsync();
            ViewBag.LeaveTypes = leaveTypes;

            return View();
        }

        // POST: Set Custom Entitlement
        [HttpPost("SetCustomEntitlement")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> SetCustomEntitlement(string employeeId, string leaveTypeId, decimal customDays, string notes = null)
        {
            try
            {
                // Debug logging
                System.Diagnostics.Debug.WriteLine($"SetCustomEntitlement called with employeeId: '{employeeId}', leaveTypeId: '{leaveTypeId}', customDays: {customDays}");

                var currentUser = await _userManager.GetUserAsync(User);
                var currentEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.ApplicationUserId == currentUser.Id);
                
                if (currentEmployee == null)
                {
                    return Json(new { success = false, message = "HR employee profile not found" });
                }

                var entitlement = await _leaveService.SetCustomEntitlementAsync(employeeId, leaveTypeId, customDays, currentEmployee.Id, notes);
                return Json(new { success = true, message = "Custom entitlement set successfully", entitlement });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message + (ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : "");
                System.Diagnostics.Debug.WriteLine($"SetCustomEntitlement ERROR: {errorMsg}");
                return Json(new { success = false, message = errorMsg });
            }
        }

        // GET: Carryover Management
        [HttpGet("ManageCarryovers")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> ManageCarryovers(int year = 0, bool useEthiopianYear = false)
        {
            if (year == 0)
            {
                if (useEthiopianYear)
                {
                    // Default to previous Ethiopian year
                    var currentEthiopianYear = EthiopianCalendarHelper.GetCurrentEthiopianDate().Year;
                    year = currentEthiopianYear - 1;
                }
                else
                {
                    // Default to previous Gregorian year
                    year = DateTime.Now.Year - 1;
                }
            }

            // Get all employees with leave balances
            var employeesWithBalances = await _context.EmployeeLeaves
                .Include(el => el.Employee)
                .Include(el => el.LeaveType)
                .Where(el => el.Year == year && el.RemainingDays > 0)
                .GroupBy(el => el.Employee)
                .Select(g => new {
                    Employee = g.Key,
                    Balances = g.ToList()
                })
                .ToListAsync();

            ViewBag.Year = year;
            ViewBag.NextYear = year + 1;
            ViewBag.UseEthiopianYear = useEthiopianYear;
            
            // Add Ethiopian year conversion info
            if (useEthiopianYear)
            {
                var ethiopianYearStart = EthiopianCalendarHelper.ToGregorianDate(year, 1, 1);
                var ethiopianYearEnd = EthiopianCalendarHelper.ToGregorianDate(year, 13, 5); // Last day of Ethiopian year
                var nextEthiopianYearStart = EthiopianCalendarHelper.ToGregorianDate(year + 1, 1, 1);
                
                ViewBag.EthiopianYearInfo = new
                {
                    EthiopianYear = year,
                    GregorianStart = ethiopianYearStart,
                    GregorianEnd = ethiopianYearEnd,
                    NextGregorianStart = nextEthiopianYearStart
                };
            }

            return View(employeesWithBalances);
        }

        // POST: Process Carryover
        [HttpPost("ProcessCarryover")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> ProcessCarryover(string employeeId, string leaveTypeId, int fromYear, int toYear, string remarks = null, bool useEthiopianYear = false)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var currentEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.ApplicationUserId == currentUser.Id);
                
                if (currentEmployee == null)
                {
                    return Json(new { success = false, message = "HR employee profile not found" });
                }

                LeaveCarryover carryover;
                if (useEthiopianYear)
                {
                    carryover = await _leaveService.ProcessEthiopianYearCarryoverAsync(employeeId, leaveTypeId, fromYear, toYear, currentEmployee.Id, remarks);
                }
                else
                {
                    carryover = await _leaveService.ProcessLeaveCarryoverAsync(employeeId, leaveTypeId, fromYear, toYear, currentEmployee.Id, remarks);
                }
                
                return Json(new { 
                    success = true, 
                    message = $"Successfully carried over {carryover.CarriedOverDays} days. {carryover.ExpiredDays} days expired.",
                    carriedOverDays = carryover.CarriedOverDays,
                    expiredDays = carryover.ExpiredDays
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Employee Years of Service
        [HttpGet("GetEmployeeYearsOfService")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetEmployeeYearsOfService(string employeeId)
        {
            try
            {
                var yearsOfService = await _leaveService.CalculateYearsOfServiceAsync(employeeId);
                var employee = await _context.Employees.FindAsync(employeeId);
                
                return Json(new { 
                    success = true, 
                    yearsOfService = yearsOfService,
                    hireDate = employee?.HireDate.ToString("yyyy-MM-dd")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Calculate Projected Entitlement
        [HttpGet("CalculateProjectedEntitlement")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> CalculateProjectedEntitlement(string employeeId, string leaveTypeId, int year)
        {
            try
            {
                var entitlement = await _leaveService.CalculateEmployeeEntitlementAsync(employeeId, leaveTypeId, year);
                var yearsOfService = await _leaveService.CalculateYearsOfServiceAsync(employeeId, new DateTime(year, 12, 31));
                
                return Json(new { 
                    success = true, 
                    entitlement = entitlement,
                    yearsOfService = yearsOfService
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 