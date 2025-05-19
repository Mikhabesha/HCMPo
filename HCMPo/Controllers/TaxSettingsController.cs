using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HCMPo.Models;
using System.Threading.Tasks;
using HCMPo.Data;

namespace HCMPo.Controllers
{
    public class TaxSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaxSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TaxSettings
        public async Task<IActionResult> Index()
        {
            return View(await _context.TaxSettings.ToListAsync());
        }

        // GET: TaxSettings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TaxSettings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Type,MinSalary,MaxSalary,Percentage,IsActive,Name")] TaxSetting taxSetting)
        {
            if (taxSetting.Type != TaxType.IncomeTax)
            {
                taxSetting.MinSalary = null;
                taxSetting.MaxSalary = null;
            }
            if (ModelState.IsValid)
            {
                _context.Add(taxSetting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(taxSetting);
        }

        // GET: TaxSettings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taxSetting = await _context.TaxSettings.FindAsync(id);
            if (taxSetting == null)
            {
                return NotFound();
            }
            return View(taxSetting);
        }

        // POST: TaxSettings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Type,MinSalary,MaxSalary,Percentage,IsActive,Name")] TaxSetting taxSetting)
        {
            if (id != taxSetting.Id)
            {
                return NotFound();
            }
            if (taxSetting.Type != TaxType.IncomeTax)
            {
                taxSetting.MinSalary = null;
                taxSetting.MaxSalary = null;
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taxSetting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaxSettingExists(taxSetting.Id))
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
            return View(taxSetting);
        }

        // GET: TaxSettings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taxSetting = await _context.TaxSettings
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taxSetting == null)
            {
                return NotFound();
            }

            return View(taxSetting);
        }

        // POST: TaxSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taxSetting = await _context.TaxSettings.FindAsync(id);
            _context.TaxSettings.Remove(taxSetting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaxSettingExists(int id)
        {
            return _context.TaxSettings.Any(e => e.Id == id);
        }
    }
} 