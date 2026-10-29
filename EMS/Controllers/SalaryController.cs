using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMS.Data;
using EMS.Models;

namespace EMS.Controllers
{
    [Authorize]
    public class SalaryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SalaryController(ApplicationDbContext context) => _context = context;

        [Authorize(Roles="Admin")]
        public async Task<IActionResult> Index()
        {
            var salaries = await _context.Salaries.Include(s=>s.Employee).OrderByDescending(s=>s.SalaryId).ToListAsync();
            return View(salaries);
        }

        [Authorize(Roles="Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Employees = await _context.Employees.Where(e=>e.IsActive).OrderBy(e=>e.FirstName).ToListAsync();
            return View(new Salary());
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles="Admin")]
        public async Task<IActionResult> Create(Salary salary)
        {
            if (salary.Deductions > salary.BasicSalary + salary.Allowances)
                ModelState.AddModelError("Deductions", "Deductions cannot exceed basic salary plus allowances.");

            if (!DateTime.TryParseExact(salary.Month + "-01", "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _))
                ModelState.AddModelError("Month", "Month must use YYYY-MM format.");

            if (ModelState.IsValid)
            {
                var exists = await _context.Salaries.AnyAsync(s=>s.EmployeeId==salary.EmployeeId && s.Month==salary.Month);
                if (exists) ModelState.AddModelError("Month", "Salary for this employee and month already exists.");
                else { _context.Salaries.Add(salary); await _context.SaveChangesAsync(); TempData["SuccessMessage"]="Salary record created successfully."; return RedirectToAction(nameof(Index)); }
            }
            ViewBag.Employees = await _context.Employees.Where(e=>e.IsActive).OrderBy(e=>e.FirstName).ToListAsync();
            return View(salary);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles="Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var salary = await _context.Salaries.FindAsync(id);
            if (salary == null) return NotFound();
            salary.Status="Approved"; salary.ApprovedDate=DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"]="Salary approved successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles="Employee")]
        public async Task<IActionResult> MySalary()
        {
            var employeeId = GetEmployeeId();
            if (employeeId == null) return Forbid();
            var salaries = await _context.Salaries.Where(s=>s.EmployeeId==employeeId && s.Status=="Approved").OrderByDescending(s=>s.Month).ToListAsync();
            return View(salaries);
        }
        private int? GetEmployeeId() => int.TryParse(User.FindFirst("EmployeeId")?.Value, out var id) ? id : null;
    }
}
