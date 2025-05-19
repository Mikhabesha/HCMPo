using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HCMPo.Data;
using HCMPo.Models;
using System.Threading.Tasks;
using System.Linq;

namespace HCMPo.Controllers
{
    public class DeductionTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeductionTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DeductionTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.DeductionTypes.OrderBy(dt => dt.Order).ToListAsync());
        }

        // GET: DeductionTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DeductionTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,DisplayName,Order,IsActive")] DeductionType deductionType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(deductionType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(deductionType);
        }

        // GET: DeductionTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deductionType = await _context.DeductionTypes.FindAsync(id);
            if (deductionType == null)
            {
                return NotFound();
            }
            return View(deductionType);
        }

        // POST: DeductionTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,DisplayName,Order,IsActive")] DeductionType deductionType)
        {
            if (id != deductionType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deductionType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeductionTypeExists(deductionType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(deductionType);
        }

        // GET: DeductionTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deductionType = await _context.DeductionTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deductionType == null)
            {
                return NotFound();
            }

            return View(deductionType);
        }

        // POST: DeductionTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deductionType = await _context.DeductionTypes.FindAsync(id);
            if (deductionType != null)
            {
                _context.DeductionTypes.Remove(deductionType);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DeductionTypeExists(int id)
        {
            return _context.DeductionTypes.Any(e => e.Id == id);
        }
    }
} 