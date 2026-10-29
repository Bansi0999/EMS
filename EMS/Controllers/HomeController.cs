using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMS.Data;
using EMS.Models;

namespace EMS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Employee"))
                return RedirectToAction(nameof(EmployeeDashboard));

            var departments = await _context.Departments
                .Include(d => d.Employees)
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalEmployees = await _context.Employees.CountAsync(),
                TotalDepartments = await _context.Departments.CountAsync(),
                TotalDesignations = await _context.Designations.CountAsync(),
                ActiveEmployees = await _context.Employees.CountAsync(e => e.IsActive),
                InactiveEmployees = await _context.Employees.CountAsync(e => !e.IsActive),
                TotalMonthlySalary = await _context.Employees.Where(e => e.IsActive).SumAsync(e => (decimal?)e.Salary) ?? 0,
                PendingLeaveRequests = await _context.LeaveRequests.CountAsync(l => l.Status == "Pending"),
                PendingLevelRequests = await _context.EmployeeLevels.CountAsync(l => l.Status == "Pending"),
                DepartmentNames = departments.Select(d => d.DepartmentName).ToList(),
                DepartmentEmployeeCounts = departments.Select(d => d.Employees != null ? d.Employees.Count : 0).ToList(),
                RecentEmployees = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Designation)
                    .OrderByDescending(e => e.EmployeeId)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }


        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> EmployeeDashboard()
        {
            if (!int.TryParse(User.FindFirst("EmployeeId")?.Value, out var employeeId)) return Forbid();
            var employee = await _context.Employees.Include(e => e.Department).Include(e => e.Designation).FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
            if (employee == null) return NotFound();
            ViewBag.PendingLeaves = await _context.LeaveRequests.CountAsync(l => l.EmployeeId == employeeId && l.Status == "Pending");
            ViewBag.ApprovedLeaves = await _context.LeaveRequests.CountAsync(l => l.EmployeeId == employeeId && l.Status == "Approved");
            ViewBag.RejectedLeaves = await _context.LeaveRequests.CountAsync(l => l.EmployeeId == employeeId && l.Status == "Rejected");
            ViewBag.PendingLevelRequests = await _context.EmployeeLevels.CountAsync(l => l.EmployeeId == employeeId && l.Status == "Pending");
            ViewBag.ApprovedSalary = await _context.Salaries.Where(s => s.EmployeeId == employeeId && s.Status == "Approved").OrderByDescending(s => s.Month).FirstOrDefaultAsync();
            ViewBag.CurrentLevel = await _context.EmployeeLevels.Include(x => x.Level).Where(x => x.EmployeeId == employeeId && x.Status == "Approved").OrderByDescending(x => x.ApprovedDate).FirstOrDefaultAsync();
            return View(employee);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }
    }
}
