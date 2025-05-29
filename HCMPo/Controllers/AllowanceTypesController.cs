using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HCMPo.Data;
using HCMPo.Models;
using Microsoft.AspNetCore.Authorization;

namespace HCMPo.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class AllowanceTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AllowanceTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AllowanceTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.AllowanceTypes.OrderBy(a => a.Name).ToListAsync());
        }

        // GET: AllowanceTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var allowanceType = await _context.AllowanceTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (allowanceType == null)
            {
                return NotFound();
            }

            return View(allowanceType);
        }

        // GET: AllowanceTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AllowanceTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,IsActive")] AllowanceType allowanceType)
        {
            if (ModelState.IsValid)
            {
                allowanceType.CreatedBy = User.Identity.Name;
                _context.Add(allowanceType);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Allowance type created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(allowanceType);
        }

        // GET: AllowanceTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var allowanceType = await _context.AllowanceTypes.FindAsync(id);
            if (allowanceType == null)
            {
                return NotFound();
            }
            return View(allowanceType);
        }

        // POST: AllowanceTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsActive")] AllowanceType allowanceType)
        {
            if (id != allowanceType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    allowanceType.ModifiedBy = User.Identity.Name;
                    allowanceType.ModifiedAt = DateTime.UtcNow;
                    _context.Update(allowanceType);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Allowance type updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AllowanceTypeExists(allowanceType.Id))
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
            return View(allowanceType);
        }

        // GET: AllowanceTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var allowanceType = await _context.AllowanceTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (allowanceType == null)
            {
                return NotFound();
            }

            return View(allowanceType);
        }

        // POST: AllowanceTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var allowanceType = await _context.AllowanceTypes.FindAsync(id);
            if (allowanceType != null)
            {
                _context.AllowanceTypes.Remove(allowanceType);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Allowance type deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AllowanceTypeExists(int id)
        {
            return _context.AllowanceTypes.Any(e => e.Id == id);
        }
    }
} 