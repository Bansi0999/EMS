using EMS.Data;
using EMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMS.Controllers
{
    [Authorize(Roles = "Employee")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(ApplicationDbContext context, IWebHostEnvironment environment)
        { _context = context; _environment = environment; }

        private bool TryGetEmployeeId(out int employeeId) => int.TryParse(User.FindFirst("EmployeeId")?.Value, out employeeId) && employeeId > 0;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!TryGetEmployeeId(out var employeeId)) return Forbid();
            var employee = await _context.Employees.Include(e => e.Department).Include(e => e.Designation)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
            if (employee == null) return NotFound("Employee profile was not found.");
            return View(ToViewModel(employee));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(EmployeeProfileViewModel model)
        {
            if (!TryGetEmployeeId(out var employeeId)) return Forbid();
            var employee = await _context.Employees.Include(e => e.Department).Include(e => e.Designation)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
            if (employee == null) return NotFound();

            // The employee may only edit personal contact details, never salary, role, department or employee code.
            if (await _context.Employees.AnyAsync(e => e.Email == model.Email && e.EmployeeId != employeeId))
                ModelState.AddModelError(nameof(model.Email), "This email address is already used by another employee.");

            if (model.PhotoFile != null && model.PhotoFile.Length > 2 * 1024 * 1024)
                ModelState.AddModelError(nameof(model.PhotoFile), "Profile picture must be 2 MB or smaller.");
            var ext = model.PhotoFile == null ? "" : Path.GetExtension(model.PhotoFile.FileName).ToLowerInvariant();
            if (model.PhotoFile != null && !new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext))
                ModelState.AddModelError(nameof(model.PhotoFile), "Upload a JPG, PNG or WEBP image.");

            if (!ModelState.IsValid)
            {
                model.EmployeeId = employee.EmployeeId; model.EmployeeCode = employee.EmployeeCode;
                model.PhotoPath = employee.PhotoPath; model.DepartmentName = employee.Department?.DepartmentName;
                model.DesignationTitle = employee.Designation?.Title; model.HireDate = employee.HireDate; model.Salary = employee.Salary;
                return View(model);
            }

            employee.FirstName = model.FirstName.Trim();
            employee.LastName = model.LastName.Trim();
            employee.Email = model.Email.Trim();
            employee.Phone = model.Phone.Trim();
            employee.Address = model.Address?.Trim();
            employee.Gender = model.Gender;
            employee.DateOfBirth = model.DateOfBirth;

            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                var folder = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "uploads", "employees");
                Directory.CreateDirectory(folder);
                var fileName = $"employee_{employee.EmployeeId}_{Guid.NewGuid():N}{ext}";
                using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.CreateNew))
                    await model.PhotoFile.CopyToAsync(stream);
                employee.PhotoPath = "/uploads/employees/" + fileName;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Your profile has been updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        private static EmployeeProfileViewModel ToViewModel(Employee e) => new EmployeeProfileViewModel
        {
            EmployeeId = e.EmployeeId, EmployeeCode = e.EmployeeCode, FirstName = e.FirstName, LastName = e.LastName,
            Email = e.Email, Phone = e.Phone, Address = e.Address, Gender = e.Gender, DateOfBirth = e.DateOfBirth,
            PhotoPath = e.PhotoPath, DepartmentName = e.Department?.DepartmentName, DesignationTitle = e.Designation?.Title,
            HireDate = e.HireDate, Salary = e.Salary
        };
    }
}
