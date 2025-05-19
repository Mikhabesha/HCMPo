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

namespace HCMPo.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class EmployeeDocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<EmployeeDocumentsController> _logger;

        public EmployeeDocumentsController(
            ApplicationDbContext context,
            IWebHostEnvironment hostingEnvironment,
            ILogger<EmployeeDocumentsController> logger)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        // GET: EmployeeDocuments/Index/5
        public async Task<IActionResult> Index(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Documents)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: EmployeeDocuments/Create/5
        public async Task<IActionResult> Create(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return NotFound();
            }

            ViewData["EmployeeId"] = employeeId;
            ViewData["EmployeeName"] = employee.FullName;
            return View();
        }

        // POST: EmployeeDocuments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeId,DocumentType,Description,ExpiryDate")] EmployeeDocument document)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var file = Request.Form.Files["DocumentFile"];
                    if (file != null && file.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "documents");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        document.Id = Guid.NewGuid().ToString();
                        document.FileName = file.FileName;
                        document.FilePath = Path.Combine("uploads", "documents", uniqueFileName);
                        document.UploadDate = DateTime.UtcNow;
                        document.UploadedBy = User.Identity.Name;
                        document.IsActive = true;

                        _context.EmployeeDocuments.Add(document);
                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Document uploaded successfully.";
                        return RedirectToAction(nameof(Index), new { employeeId = document.EmployeeId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please select a file to upload.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading document");
                    ModelState.AddModelError("", "An error occurred while uploading the document. Please try again.");
                }
            }

            var employee = await _context.Employees.FindAsync(document.EmployeeId);
            ViewData["EmployeeId"] = document.EmployeeId;
            ViewData["EmployeeName"] = employee?.FullName;
            return View(document);
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
    }
} 