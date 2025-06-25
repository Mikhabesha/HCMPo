using HCMPo.Data;
using HCMPo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace HCMPo.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class JobTitlesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobTitlesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: JobTitles
        public async Task<IActionResult> Index()
        {
            var jobTitles = await _context.JobTitles
                .Include(j => j.OrganizationUnit)
                .OrderBy(j => j.Title)
                .ToListAsync();
            return View(jobTitles);
        }

        // GET: JobTitles/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            
            var jobTitle = await _context.JobTitles
                .Include(j => j.OrganizationUnit)
                .FirstOrDefaultAsync(j => j.Id == id);
                
            if (jobTitle == null) return NotFound();
            
            return View(jobTitle);
        }

        // GET: JobTitles/Create
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // If admin, show all org units, otherwise filter by user's org unit
            var query = _context.OrganizationUnits.AsQueryable();
            if (!isAdmin)
            {
                var userOrgUnitId = await _context.Employees
                    .Where(e => e.ApplicationUserId == userId)
                    .Select(e => e.OrganizationUnitId)
                    .FirstOrDefaultAsync();

                if (userOrgUnitId != null)
                {
                    query = query.Where(ou => ou.Id == userOrgUnitId);
                }
            }

            ViewBag.OrganizationUnits = new SelectList(
                await query.OrderBy(o => o.Name).ToListAsync(),
                "Id",
                "Name"
            );

            return View();
        }

        // POST: JobTitles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobTitle jobTitle)
        {
            if (ModelState.IsValid)
            {
                jobTitle.Id = System.Guid.NewGuid().ToString();
                jobTitle.CreatedAt = System.DateTime.UtcNow;
                jobTitle.CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                _context.JobTitles.Add(jobTitle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdown if validation fails
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var query = _context.OrganizationUnits.AsQueryable();
            
            if (!isAdmin)
            {
                var userOrgUnitId = await _context.Employees
                    .Where(e => e.ApplicationUserId == userId)
                    .Select(e => e.OrganizationUnitId)
                    .FirstOrDefaultAsync();

                if (userOrgUnitId != null)
                {
                    query = query.Where(ou => ou.Id == userOrgUnitId);
                }
            }

            ViewBag.OrganizationUnits = new SelectList(
                await query.OrderBy(o => o.Name).ToListAsync(),
                "Id",
                "Name",
                jobTitle.OrganizationUnitId
            );

            return View(jobTitle);
        }

        // GET: JobTitles/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            
            var jobTitle = await _context.JobTitles.FindAsync(id);
            if (jobTitle == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var query = _context.OrganizationUnits.AsQueryable();
            
            if (!isAdmin)
            {
                var userOrgUnitId = await _context.Employees
                    .Where(e => e.ApplicationUserId == userId)
                    .Select(e => e.OrganizationUnitId)
                    .FirstOrDefaultAsync();

                if (userOrgUnitId != null)
                {
                    query = query.Where(ou => ou.Id == userOrgUnitId);
                }
            }

            ViewBag.OrganizationUnits = new SelectList(
                await query.OrderBy(o => o.Name).ToListAsync(),
                "Id",
                "Name",
                jobTitle.OrganizationUnitId
            );

            return View(jobTitle);
        }

        // POST: JobTitles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, JobTitle jobTitle)
        {
            if (id != jobTitle.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    jobTitle.ModifiedAt = System.DateTime.UtcNow;
                    jobTitle.ModifiedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    
                    _context.Update(jobTitle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.JobTitles.Any(e => e.Id == jobTitle.Id))
                        return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdown if validation fails
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var query = _context.OrganizationUnits.AsQueryable();
            
            if (!isAdmin)
            {
                var userOrgUnitId = await _context.Employees
                    .Where(e => e.ApplicationUserId == userId)
                    .Select(e => e.OrganizationUnitId)
                    .FirstOrDefaultAsync();

                if (userOrgUnitId != null)
                {
                    query = query.Where(ou => ou.Id == userOrgUnitId);
                }
            }

            ViewBag.OrganizationUnits = new SelectList(
                await query.OrderBy(o => o.Name).ToListAsync(),
                "Id",
                "Name",
                jobTitle.OrganizationUnitId
            );

            return View(jobTitle);
        }

        // GET: JobTitles/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            
            var jobTitle = await _context.JobTitles
                .Include(j => j.OrganizationUnit)
                .FirstOrDefaultAsync(j => j.Id == id);
                
            if (jobTitle == null) return NotFound();
            
            return View(jobTitle);
        }

        // POST: JobTitles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var jobTitle = await _context.JobTitles.FindAsync(id);
            if (jobTitle != null)
            {
                _context.JobTitles.Remove(jobTitle);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: JobTitles/GetByOrgUnit/5
        [HttpGet]
        public async Task<IActionResult> GetByOrgUnit(string orgUnitId)
        {
            if (string.IsNullOrEmpty(orgUnitId))
                return Json(new List<SelectListItem>());

            // Get the parent org unit ID
            var parentOrgUnitId = await _context.OrganizationUnits
                .Where(ou => ou.Id == orgUnitId)
                .Select(ou => ou.ParentId)
                .FirstOrDefaultAsync();

            // Get job titles for the parent org unit
            var jobTitles = await _context.JobTitles
                .Where(jt => jt.OrganizationUnitId == parentOrgUnitId && jt.IsActive)
                .OrderBy(jt => jt.Title)
                .Select(jt => new SelectListItem
                {
                    Value = jt.Id,
                    Text = jt.Title
                })
                .ToListAsync();

            return Json(jobTitles);
        }
    }
} 