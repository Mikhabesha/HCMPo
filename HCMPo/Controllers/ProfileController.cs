using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HCMPo.Models;
using HCMPo.Data;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using HCMPo.Services;
using Microsoft.AspNetCore.SignalR;

namespace HCMPo.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProfileController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<ProfileController> logger,
            IWebHostEnvironment hostingEnvironment,
            INotificationService notificationService,
            IHubContext<NotificationHub> hubContext)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        // GET: Profile/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Get employee profile if linked
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.ApplicationUserId == user.Id);

            var profileViewModel = new ProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                LeaveRequestAlerts = user.LeaveRequestAlerts,
                PayrollAlerts = user.PayrollAlerts,
                AttendanceAlerts = user.AttendanceAlerts,
                Employee = employee
            };

            return View(profileViewModel);
        }

        // GET: Profile/UploadPhoto
        public async Task<IActionResult> UploadPhoto()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.ApplicationUserId == user.Id);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "No employee profile linked to your account.";
                return RedirectToAction(nameof(Index));
            }

            return View(new PhotoUploadViewModel { EmployeeId = employee.Id });
        }

        // POST: Profile/UploadPhoto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhoto(PhotoUploadViewModel model)
        {
            _logger.LogInformation("UploadPhoto POST method called");
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.ApplicationUserId == user.Id);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "No employee profile linked to your account.";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation("UploadPhoto: User {UserId}, Employee {EmployeeId}, ModelState valid: {IsValid}", 
                user.Id, employee.Id, ModelState.IsValid);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("UploadPhoto: ModelState is invalid. Errors: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return View(model);
            }

            try
            {
                var file = model.PhotoFile;
                if (file != null && file.Length > 0)
                {
                    _logger.LogInformation("UploadPhoto: File received - Name: {FileName}, Size: {FileSize}, ContentType: {ContentType}", 
                        file.FileName, file.Length, file.ContentType);

                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        _logger.LogWarning("UploadPhoto: Invalid file extension: {Extension}", fileExtension);
                        ModelState.AddModelError("PhotoFile", "Only JPG, PNG, and GIF files are allowed.");
                        return View(model);
                    }

                    // Validate file size (max 5MB)
                    if (file.Length > 5 * 1024 * 1024)
                    {
                        _logger.LogWarning("UploadPhoto: File too large: {Size} bytes", file.Length);
                        ModelState.AddModelError("PhotoFile", "File size must be less than 5MB.");
                        return View(model);
                    }

                    var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "photos");
                    _logger.LogInformation("UploadPhoto: Upload folder path: {Path}", uploadsFolder);
                    
                    try
                    {
                        if (!Directory.Exists(uploadsFolder))
                        {
                            _logger.LogInformation("Creating directory: {dir}", uploadsFolder);
                            Directory.CreateDirectory(uploadsFolder);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create directory {dir}", uploadsFolder);
                        ModelState.AddModelError("", "An error occurred while creating the photo directory on the server. Please check application permissions.");
                        return View(model);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    _logger.LogInformation("UploadPhoto: File will be saved to: {FilePath}", filePath);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    _logger.LogInformation("UploadPhoto: File saved successfully");

                    // Create document record
                    var document = new EmployeeDocument
                    {
                        Id = Guid.NewGuid().ToString(),
                        EmployeeId = employee.Id,
                        DocumentType = "Profile Photo",
                        Description = "Employee profile photo",
                        FileName = file.FileName,
                        FilePath = $"uploads/photos/{uniqueFileName}",
                        UploadDate = DateTime.UtcNow,
                        UploadedBy = user.UserName,
                        IsActive = true
                    };

                    _context.EmployeeDocuments.Add(document);
                    _logger.LogInformation("UploadPhoto: Document record created with ID: {DocumentId}", document.Id);

                    // Update employee photo URL
                    employee.PhotoUrl = $"uploads/photos/{uniqueFileName}";
                    _context.Employees.Update(employee);
                    _logger.LogInformation("UploadPhoto: Employee PhotoUrl updated to: {PhotoUrl}", employee.PhotoUrl);

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("UploadPhoto: Database changes saved successfully");

                    TempData["SuccessMessage"] = "Profile photo uploaded successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogWarning("UploadPhoto: No file received or file is empty");
                    ModelState.AddModelError("PhotoFile", "Please select a photo to upload.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile photo for user {UserId}", user.Id);
                ModelState.AddModelError("", "An error occurred while uploading the photo. Please try again.");
            }

            return View(model);
        }

        // GET: Profile/ViewPhoto
        public async Task<IActionResult> ViewPhoto()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Documents.Where(d => d.DocumentType == "Profile Photo" && d.IsActive))
                .FirstOrDefaultAsync(e => e.ApplicationUserId == user.Id);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "No employee profile linked to your account.";
                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }

        // POST: Profile/DeletePhoto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhoto(string documentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.ApplicationUserId == user.Id);

            if (employee == null)
            {
                return Json(new { success = false, message = "No employee profile found" });
            }

            try
            {
                var document = await _context.EmployeeDocuments
                    .FirstOrDefaultAsync(d => d.Id == documentId && d.EmployeeId == employee.Id && d.DocumentType == "Profile Photo");

                if (document == null)
                {
                    return Json(new { success = false, message = "Photo not found" });
                }

                // Delete physical file
                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, document.FilePath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Remove document record
                _context.EmployeeDocuments.Remove(document);

                // Clear employee photo URL if this was the current photo
                if (employee.PhotoUrl == document.FilePath)
                {
                    employee.PhotoUrl = null;
                    _context.Employees.Update(employee);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Photo deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile photo for user {UserId}", user.Id);
                return Json(new { success = false, message = "An error occurred while deleting the photo" });
            }
        }

        // GET: Profile/Settings
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var settingsViewModel = new SettingsViewModel
            {
                UserId = user.Id,
                LeaveRequestAlerts = user.LeaveRequestAlerts,
                PayrollAlerts = user.PayrollAlerts,
                AttendanceAlerts = user.AttendanceAlerts,
                Theme = user.Theme ?? "light",
                Language = user.Language ?? "en"
            };

            return View(settingsViewModel);
        }

        // POST: Profile/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                user.LeaveRequestAlerts = model.LeaveRequestAlerts;
                user.PayrollAlerts = model.PayrollAlerts;
                user.AttendanceAlerts = model.AttendanceAlerts;
                user.Theme = model.Theme;
                user.Language = model.Language;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Settings updated successfully!";
                    return RedirectToAction(nameof(Settings));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user settings for user {UserId}", user.Id);
                ModelState.AddModelError("", "An error occurred while updating settings.");
            }

            return View(model);
        }

        // GET: Profile/Notifications
        public async Task<IActionResult> Notifications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Get user's notifications
            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();

            var notificationsViewModel = new NotificationsViewModel
            {
                Notifications = notifications,
                LeaveRequestAlerts = user.LeaveRequestAlerts,
                PayrollAlerts = user.PayrollAlerts,
                AttendanceAlerts = user.AttendanceAlerts
            };

            return View(notificationsViewModel);
        }

        // POST: Profile/MarkNotificationAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationAsRead(string notificationId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == user.Id);

            if (notification == null)
            {
                return Json(new { success = false, message = "Notification not found" });
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // POST: Profile/MarkAllNotificationsAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var unreadNotifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly: true);

            foreach (var notification in unreadNotifications)
            {
                await _notificationService.MarkAsReadAsync(notification.Id);
            }
            
            TempData["SuccessMessage"] = "All notifications marked as read.";
            return RedirectToAction(nameof(Notifications));
        }

        // POST: Profile/UpdateNotificationSettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotificationSettings(bool leaveRequestAlerts, bool payrollAlerts, bool attendanceAlerts)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            try
            {
                user.LeaveRequestAlerts = leaveRequestAlerts;
                user.PayrollAlerts = payrollAlerts;
                user.AttendanceAlerts = attendanceAlerts;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "Notification settings updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update notification settings." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification settings for user {UserId}", user.Id);
                return Json(new { success = false, message = "An error occurred while updating settings." });
            }
        }

        // GET: Profile/TestUpload
        public IActionResult TestUpload()
        {
            return View();
        }

        // POST: Profile/TestUpload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestUpload(IFormFile testFile)
        {
            _logger.LogInformation("TestUpload POST method called");
            
            if (testFile != null && testFile.Length > 0)
            {
                _logger.LogInformation("TestUpload: File received - Name: {FileName}, Size: {FileSize}", 
                    testFile.FileName, testFile.Length);
                
                return Json(new { success = true, fileName = testFile.FileName, size = testFile.Length });
            }
            else
            {
                _logger.LogWarning("TestUpload: No file received");
                return Json(new { success = false, message = "No file received" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadNotificationSummary()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Json(new { count = 0, type = "Info" });
            }
            var summary = await _notificationService.GetUnreadNotificationSummaryAsync(userId);
            return Json(new { count = summary.count, type = summary.mostRecentType });
        }

        [HttpGet]
        public IActionResult NotificationsDropdown()
        {
            return ViewComponent("NotificationsDropdown");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }

    public class ProfileViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool LeaveRequestAlerts { get; set; }
        public bool PayrollAlerts { get; set; }
        public bool AttendanceAlerts { get; set; }
        public Employee Employee { get; set; }
    }

    public class SettingsViewModel
    {
        public string UserId { get; set; }
        public bool LeaveRequestAlerts { get; set; }
        public bool PayrollAlerts { get; set; }
        public bool AttendanceAlerts { get; set; }
        public string Theme { get; set; }
        public string Language { get; set; }
    }

    public class NotificationsViewModel
    {
        public List<Notification> Notifications { get; set; }
        public bool LeaveRequestAlerts { get; set; }
        public bool PayrollAlerts { get; set; }
        public bool AttendanceAlerts { get; set; }
    }

    public class PhotoUploadViewModel
    {
        public string EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Please select a photo to upload")]
        public IFormFile PhotoFile { get; set; }
    }
} 