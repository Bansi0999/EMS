using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMS.Data;
using EMS.Models;

namespace EMS.Controllers
{
    [Authorize]
    public class LeaveController : Controller
    {
        private readonly ApplicationDbContext _context;
        public LeaveController(ApplicationDbContext context) => _context=context;

        [Authorize(Roles="Admin")]
        public async Task<IActionResult> Index() => View(await _context.LeaveRequests.Include(l=>l.Employee).OrderByDescending(l=>l.AppliedDate).ToListAsync());

        [Authorize(Roles="Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id) => await SetStatus(id,"Approved");

        [Authorize(Roles="Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id) => await SetStatus(id,"Rejected");

        [Authorize(Roles="Employee")]
        public IActionResult Apply() => View(new LeaveRequest { FromDate=DateTime.Today, ToDate=DateTime.Today });

        [Authorize(Roles="Employee")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(LeaveRequest request)
        {
            var employeeId=GetEmployeeId();
            if(employeeId==null) return Forbid();
            if(request.ToDate < request.FromDate) ModelState.AddModelError("ToDate","To date must be on or after from date.");
            if(request.FromDate.Date < DateTime.Today) ModelState.AddModelError("FromDate","Leave cannot start in the past.");
            var leaveDays = (request.ToDate.Date - request.FromDate.Date).Days + 1;
            if(leaveDays > 30) ModelState.AddModelError("ToDate","A single leave request cannot exceed 30 days.");
            var overlaps = await _context.LeaveRequests.AnyAsync(l => l.EmployeeId == employeeId.Value && l.Status != "Rejected" && request.FromDate.Date <= l.ToDate.Date && request.ToDate.Date >= l.FromDate.Date);
            if(overlaps) ModelState.AddModelError(string.Empty,"You already have a pending or approved leave overlapping these dates.");
            if(ModelState.IsValid){ request.EmployeeId=employeeId.Value; request.Status="Pending"; request.AppliedDate=DateTime.Now; _context.LeaveRequests.Add(request); await _context.SaveChangesAsync(); TempData["SuccessMessage"]="Leave application submitted."; return RedirectToAction(nameof(MyLeaves)); }
            return View(request);
        }

        [Authorize(Roles="Employee")]
        public async Task<IActionResult> MyLeaves(){ var id=GetEmployeeId(); if(id==null)return Forbid(); return View(await _context.LeaveRequests.Where(l=>l.EmployeeId==id).OrderByDescending(l=>l.AppliedDate).ToListAsync()); }

        private async Task<IActionResult> SetStatus(int id,string status){ var leave=await _context.LeaveRequests.FindAsync(id); if(leave==null)return NotFound(); leave.Status=status; leave.ActionDate=DateTime.Now; await _context.SaveChangesAsync(); TempData["SuccessMessage"]=$"Leave {status.ToLowerInvariant()} successfully."; return RedirectToAction(nameof(Index)); }
        private int? GetEmployeeId()=>int.TryParse(User.FindFirst("EmployeeId")?.Value,out var id)?id:null;
    }
}
