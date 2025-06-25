using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using HCMPo.Models;
using Microsoft.Extensions.Logging;
using HCMPo.Data;
using System.Linq;
using HCMPo.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;

namespace HCMPo.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class EmployeeDocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<EmployeeDocumentsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeDocumentsController(
            ApplicationDbContext context,
            IWebHostEnvironment hostingEnvironment,
            ILogger<EmployeeDocumentsController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: EmployeeDocuments (List all employees)
        public async Task<IActionResult> Index(string employeeId = null)
        {
            _logger.LogInformation("Index action called with employeeId: {EmployeeId}", employeeId);
            
            if (!string.IsNullOrEmpty(employeeId))
            {
                // Show documents for specific employee
            var employee = await _context.Employees
                .Include(e => e.Documents)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
            {
                    _logger.LogWarning("Employee not found with ID: {EmployeeId}", employeeId);
                return NotFound();
            }

                // Check access control for profile photos
                var currentUser = await _userManager.GetUserAsync(User);
                var currentEmployee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.ApplicationUserId == currentUser.Id);

                // Filter out profile photos if user is not admin and not viewing their own documents
                if (!User.IsInRole("Admin") && currentEmployee?.Id != employeeId)
                {
                    employee.Documents = employee.Documents?.Where(d => d.DocumentType != "Profile Photo").ToList();
                }

                _logger.LogInformation("Found employee: {EmployeeName} with {DocumentCount} documents", 
                    employee.FullName, employee.Documents?.Count ?? 0);

                ViewBag.ViewMode = "Documents";
            return View(employee);
            }
            else
            {
                // Show list of all employees
                var employees = await _context.Employees
                    .Include(e => e.Documents)
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .ToListAsync();

                ViewBag.ViewMode = "Employees";
                ViewBag.Employees = employees;
                return View(employees);
            }
        }

        // GET: EmployeeDocuments/Create/5
        public async Task<IActionResult> Create(string employeeId)
        {
            var employees = await _context.Employees
                .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                .Select(e => new { e.Id, FullName = e.FirstName + " " + e.LastName })
                .ToListAsync();
            ViewBag.Employees = new SelectList(employees, "Id", "FullName", employeeId);

            if (!string.IsNullOrEmpty(employeeId))
            {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return NotFound();
            }
                ViewData["EmployeeName"] = employee.FullName;
            }
            else
            {
                ViewData["EmployeeName"] = null;
            }
            ViewData["EmployeeId"] = employeeId;
            return View(new EmployeeDocumentUploadViewModel { EmployeeId = employeeId });
        }

        // POST: EmployeeDocuments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeDocumentUploadViewModel model)
        {
            _logger.LogInformation("Create POST action called for employee: {EmployeeId}", model.EmployeeId);

            if (ModelState.IsValid)
            {
                try
                {
                    var file = model.DocumentFile;
                    if (file != null && file.Length > 0)
                    {
                        _logger.LogInformation("File received: {FileName}, Size: {FileSize}", file.FileName, file.Length);

                        var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "documents");
                        _logger.LogInformation("Uploads folder path: {UploadsFolder}", uploadsFolder);

                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                            _logger.LogInformation("Created uploads directory: {UploadsFolder}", uploadsFolder);
                        }

                        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        _logger.LogInformation("Full file path: {FilePath}", filePath);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        _logger.LogInformation("File saved successfully to: {FilePath}", filePath);

                        var document = new EmployeeDocument
                        {
                            Id = Guid.NewGuid().ToString(),
                            EmployeeId = model.EmployeeId,
                            DocumentType = model.DocumentType,
                            Description = model.Description,
                            ExpiryDate = model.ExpiryDate,
                            FileName = file.FileName,
                            FilePath = Path.Combine("uploads", "documents", uniqueFileName),
                            UploadDate = DateTime.UtcNow,
                            UploadedBy = User.Identity.Name,
                            IsActive = true
                        };

                        _context.EmployeeDocuments.Add(document);
                        var saveResult = await _context.SaveChangesAsync();
                        _logger.LogInformation("Document saved to database. SaveChanges result: {SaveResult}", saveResult);

                        TempData["SuccessMessage"] = "Document uploaded successfully.";
                        return RedirectToAction(nameof(Index), new { employeeId = model.EmployeeId });
                    }
                    else
                    {
                        _logger.LogWarning("No file was uploaded");
                        ModelState.AddModelError("", "Please select a file to upload.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading document");
                    ModelState.AddModelError("", "An error occurred while uploading the document. Please try again.");
                }
            }
            else
            {
                _logger.LogWarning("ModelState is invalid. Errors: {ModelStateErrors}", 
                    string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)));
            }

            var employee = await _context.Employees.FindAsync(model.EmployeeId);
            ViewData["EmployeeId"] = model.EmployeeId;
            ViewData["EmployeeName"] = employee?.FullName;
            return View(model);
        }

        // GET: EmployeeDocuments/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.EmployeeDocuments
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // POST: EmployeeDocuments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var document = await _context.EmployeeDocuments.FindAsync(id);
            if (document != null)
            {
                try
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, document.FilePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    _context.EmployeeDocuments.Remove(document);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Document deleted successfully.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting document");
                    TempData["ErrorMessage"] = "An error occurred while deleting the document.";
                }
            }

            return RedirectToAction(nameof(Index), new { employeeId = document?.EmployeeId });
        }

        // GET: EmployeeDocuments/Download/5
        public async Task<IActionResult> Download(string id)
        {
            var document = await _context.EmployeeDocuments.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, document.FilePath);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/octet-stream", document.FileName);
        }

        // GET: EmployeeDocuments/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var document = await _context.EmployeeDocuments
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (document == null)
            {
                return NotFound();
            }
            return View(document);
        }
    }
} 