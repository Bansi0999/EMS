using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EMS.Data;
using EMS.Models;

namespace EMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DesignationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DesignationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            var query = _context.Designations
                .Include(d => d.Department)
                .Include(d => d.Employees)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(d => d.Title.Contains(searchString));
            }

            ViewBag.SearchString = searchString;
            return View(await query.OrderBy(d => d.Title).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = new SelectList(await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync(), "DepartmentId", "DepartmentName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Designation designation)
        {
            if (await _context.Designations.AnyAsync(d => d.Title == designation.Title && d.DepartmentId == designation.DepartmentId))
            {
                ModelState.AddModelError("Title", "This designation already exists in the selected department.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(designation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Designation '{designation.Title}' was created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = new SelectList(await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync(), "DepartmentId", "DepartmentName", designation.DepartmentId);
            return View(designation);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var designation = await _context.Designations.FindAsync(id);
            if (designation == null) return NotFound();

            ViewBag.Departments = new SelectList(await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync(), "DepartmentId", "DepartmentName", designation.DepartmentId);
            return View(designation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Designation designation)
        {
            if (id != designation.DesignationId) return NotFound();

            if (await _context.Designations.AnyAsync(d => d.Title == designation.Title && d.DepartmentId == designation.DepartmentId && d.DesignationId != id))
            {
                ModelState.AddModelError("Title", "This designation already exists in the selected department.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(designation);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Designation '{designation.Title}' was updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Designations.AnyAsync(d => d.DesignationId == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = new SelectList(await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync(), "DepartmentId", "DepartmentName", designation.DepartmentId);
            return View(designation);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var designation = await _context.Designations
                .Include(d => d.Department)
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.DesignationId == id);

            if (designation == null) return NotFound();
            return View(designation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var designation = await _context.Designations
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.DesignationId == id);

            if (designation != null)
            {
                if (designation.Employees != null && designation.Employees.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete this designation because it has employees assigned to it.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Designations.Remove(designation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Designation '{designation.Title}' was deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
