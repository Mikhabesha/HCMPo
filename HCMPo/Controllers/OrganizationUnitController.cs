using HCMPo.Data;
using HCMPo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HCMPo.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class OrganizationUnitController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OrganizationUnitController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: OrganizationUnit
        public async Task<IActionResult> Index()
        {
            var units = await _context.OrganizationUnits.OrderBy(u => u.Name).ToListAsync();
            return View(units);
        }

        // GET: OrganizationUnit/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            var unit = await _context.OrganizationUnits.FirstOrDefaultAsync(u => u.Id == id);
            if (unit == null) return NotFound();
            return View(unit);
        }

        // GET: OrganizationUnit/Create
        public IActionResult Create()
        {
            ViewBag.AllUnits = _context.OrganizationUnits.OrderBy(u => u.Name).ToList();
            return View();
        }

        // POST: OrganizationUnit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrganizationUnit unit)
        {
            if (ModelState.IsValid)
            {
                unit.Id = System.Guid.NewGuid().ToString();
                unit.CreatedAt = System.DateTime.UtcNow;
                _context.OrganizationUnits.Add(unit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.AllUnits = _context.OrganizationUnits.OrderBy(u => u.Name).ToList();
            return View(unit);
        }

        // GET: OrganizationUnit/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var unit = await _context.OrganizationUnits.FindAsync(id);
            if (unit == null) return NotFound();
            ViewBag.AllUnits = _context.OrganizationUnits.Where(u => u.Id != id).OrderBy(u => u.Name).ToList();
            return View(unit);
        }

        // POST: OrganizationUnit/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, OrganizationUnit unit)
        {
            if (id != unit.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    unit.ModifiedAt = System.DateTime.UtcNow;
                    _context.Update(unit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.OrganizationUnits.Any(e => e.Id == unit.Id))
                        return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.AllUnits = _context.OrganizationUnits.Where(u => u.Id != id).OrderBy(u => u.Name).ToList();
            return View(unit);
        }

        // GET: OrganizationUnit/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var unit = await _context.OrganizationUnits.FirstOrDefaultAsync(u => u.Id == id);
            if (unit == null) return NotFound();
            return View(unit);
        }

        // POST: OrganizationUnit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var unit = await _context.OrganizationUnits.FindAsync(id);
            if (unit != null)
            {
                _context.OrganizationUnits.Remove(unit);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
} 