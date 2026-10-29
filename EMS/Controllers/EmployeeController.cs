using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EMS.Data;
using EMS.Models;
using EMS.Helpers;

namespace EMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private const int PageSize = 8;

        public EmployeeController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Employee
        public async Task<IActionResult> Index(string? searchString, int? departmentId, string? status, int page = 1)
        {
            var employeesQuery = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                employeesQuery = employeesQuery.Where(e =>
                    e.FirstName.Contains(searchString) ||
                    e.LastName.Contains(searchString) ||
                    e.EmployeeCode.Contains(searchString) ||
                    e.Email.Contains(searchString));
            }

            if (departmentId.HasValue && departmentId > 0)
            {
                employeesQuery = employeesQuery.Where(e => e.DepartmentId == departmentId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status == "Active";
                employeesQuery = employeesQuery.Where(e => e.IsActive == isActive);
            }

            int totalItems = await employeesQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(totalPages, 1)));

            var employees = await employeesQuery
                .OrderBy(e => e.EmployeeId)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.DepartmentId = departmentId;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "DepartmentName", departmentId);

            return View(employees);
        }

        // GET: Employee/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        // GET: Employee/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            var employee = new Employee
            {
                EmployeeCode = await GenerateEmployeeCode(),
                HireDate = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-25)
            };
            return View(employee);
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (await _context.Employees.AnyAsync(e => e.Email == employee.Email))
            {
                ModelState.AddModelError("Email", "This email is already used by another employee.");
            }

            ValidateEmploymentDates(employee);
            await ValidateDepartmentDesignation(employee);

            if (await _context.Employees.AnyAsync(e => e.EmployeeCode == employee.EmployeeCode))
            {
                ModelState.AddModelError("EmployeeCode", "This employee code already exists.");
            }

            if (ModelState.IsValid)
            {
                if (employee.PhotoFile != null)
                {
                    try { employee.PhotoPath = await SavePhoto(employee.PhotoFile); }
                    catch (InvalidOperationException ex) { ModelState.AddModelError("PhotoFile", ex.Message); }
                }

                if (!ModelState.IsValid)
                {
                    await PopulateDropdowns(employee.DepartmentId);
                    return View(employee);
                }

                _context.Add(employee);
                await _context.SaveChangesAsync();

                if (!await _context.Users.AnyAsync(u => u.EmployeeId == employee.EmployeeId))
                {
                    _context.Users.Add(new User
                    {
                        Username = employee.EmployeeCode,
                        PasswordHash = PasswordHelper.HashPassword("Employee@123"),
                        FullName = employee.FullName,
                        Role = "Employee",
                        EmployeeId = employee.EmployeeId
                    });
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Employee '{employee.FullName}' created. Login: {employee.EmployeeCode} / Employee@123";
                }
                else TempData["SuccessMessage"] = $"Employee '{employee.FullName}' was created successfully.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(employee.DepartmentId);
            return View(employee);
        }

        // GET: Employee/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            await PopulateDropdowns(employee.DepartmentId);
            return View(employee);
        }

        // POST: Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.EmployeeId) return NotFound();

            if (await _context.Employees.AnyAsync(e => e.Email == employee.Email && e.EmployeeId != id))
            {
                ModelState.AddModelError("Email", "This email is already used by another employee.");
            }

            ValidateEmploymentDates(employee);
            await ValidateDepartmentDesignation(employee);

            if (await _context.Employees.AnyAsync(e => e.EmployeeCode == employee.EmployeeCode && e.EmployeeId != id))
            {
                ModelState.AddModelError("EmployeeCode", "This employee code already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEmployee = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeId == id);
                    if (existingEmployee == null) return NotFound();

                    if (employee.PhotoFile != null)
                    {
                        try
                        {
                            employee.PhotoPath = await SavePhoto(employee.PhotoFile);
                            DeleteOldPhoto(existingEmployee.PhotoPath);
                        }
                        catch (InvalidOperationException ex)
                        {
                            ModelState.AddModelError("PhotoFile", ex.Message);
                            await PopulateDropdowns(employee.DepartmentId);
                            return View(employee);
                        }
                    }
                    else
                    {
                        employee.PhotoPath = existingEmployee.PhotoPath;
                    }

                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Employee '{employee.FullName}' was updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Employees.AnyAsync(e => e.EmployeeId == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(employee.DepartmentId);
            return View(employee);
        }

        // GET: Employee/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST: Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                DeleteOldPhoto(employee.PhotoPath);
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Employee '{employee.FullName}' was deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Employee/Designations?departmentId=2  (AJAX for cascading dropdown)
        [HttpGet]
        public async Task<JsonResult> GetDesignationsByDepartment(int departmentId)
        {
            var designations = await _context.Designations
                .Where(d => d.DepartmentId == departmentId)
                .OrderBy(d => d.Title)
                .Select(d => new { d.DesignationId, d.Title })
                .ToListAsync();

            return Json(designations);
        }

        private async Task PopulateDropdowns(int? selectedDepartmentId = null)
        {
            ViewBag.Departments = new SelectList(await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync(), "DepartmentId", "DepartmentName", selectedDepartmentId);

            var designationsQuery = _context.Designations.AsQueryable();
            if (selectedDepartmentId.HasValue)
                designationsQuery = designationsQuery.Where(d => d.DepartmentId == selectedDepartmentId);

            ViewBag.Designations = new SelectList(await designationsQuery.OrderBy(d => d.Title).ToListAsync(), "DesignationId", "Title");
        }

        private async Task<string> GenerateEmployeeCode()
        {
            int count = await _context.Employees.CountAsync() + 1;
            string code;
            do
            {
                code = $"EMP{count:D4}";
                count++;
            } while (await _context.Employees.AnyAsync(e => e.EmployeeCode == code));

            return code;
        }

        private async Task<string> SavePhoto(IFormFile photo)
        {
            if (photo.Length == 0) throw new InvalidOperationException("Please select a valid image file.");
            if (photo.Length > 2 * 1024 * 1024) throw new InvalidOperationException("Employee photo must be 2 MB or smaller.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Only image files (.jpg, .jpeg, .png, .gif) are allowed.");

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "employees");
            Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(fileStream);
            return $"/images/employees/{uniqueFileName}";
        }

        private void ValidateEmploymentDates(Employee employee)
        {
            if (employee.DateOfBirth.Date > DateTime.Today)
                ModelState.AddModelError("DateOfBirth", "Date of birth cannot be in the future.");
            if (employee.HireDate.Date > DateTime.Today)
                ModelState.AddModelError("HireDate", "Hire date cannot be in the future.");
            if (employee.HireDate.Date < employee.DateOfBirth.Date)
                ModelState.AddModelError("HireDate", "Hire date must be after the date of birth.");
        }

        private async Task ValidateDepartmentDesignation(Employee employee)
        {
            var designation = await _context.Designations.FirstOrDefaultAsync(d => d.DesignationId == employee.DesignationId);
            if (designation == null)
                ModelState.AddModelError("DesignationId", "Please select a valid designation.");
            else if (designation.DepartmentId != employee.DepartmentId)
                ModelState.AddModelError("DesignationId", "Selected designation does not belong to the selected department.");
        }

        private void DeleteOldPhoto(string? photoPath)
        {
            if (string.IsNullOrEmpty(photoPath)) return;

            var fullPath = Path.Combine(_environment.WebRootPath, photoPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}
