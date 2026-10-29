using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMS.Data;
using EMS.Models;

namespace EMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            var query = _context.Departments
                .Include(d => d.Employees)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(d => d.DepartmentName.Contains(searchString));
            }

            ViewBag.SearchString = searchString;
            return View(await query.OrderBy(d => d.DepartmentName).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Employees)
                .Include(d => d.Designations)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null) return NotFound();
            return View(department);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (await _context.Departments.AnyAsync(d => d.DepartmentName == department.DepartmentName))
            {
                ModelState.AddModelError("DepartmentName", "This department already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Department '{department.DepartmentName}' was created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();
            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.DepartmentId) return NotFound();

            if (await _context.Departments.AnyAsync(d => d.DepartmentName == department.DepartmentName && d.DepartmentId != id))
            {
                ModelState.AddModelError("DepartmentName", "This department already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Department '{department.DepartmentName}' was updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Departments.AnyAsync(d => d.DepartmentId == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null) return NotFound();
            return View(department);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department != null)
            {
                if (department.Employees != null && department.Employees.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete this department because it has employees assigned to it.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Department '{department.DepartmentName}' was deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
