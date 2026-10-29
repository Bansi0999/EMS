using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMS.Data;
using EMS.Models;

namespace EMS.Controllers
{
    [Authorize]
    public class LevelController : Controller
    {
        private readonly ApplicationDbContext _context;
        public LevelController(ApplicationDbContext context)=>_context=context;

        [Authorize(Roles="Admin")]
        public async Task<IActionResult> Index()=>View(await _context.EmployeeLevels.Include(e=>e.Employee).Include(e=>e.Level).OrderByDescending(e=>e.RequestedDate).ToListAsync());

        [Authorize(Roles="Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id){var r=await _context.EmployeeLevels.FindAsync(id);if(r==null)return NotFound();r.Status="Approved";r.ApprovedDate=DateTime.Now;await _context.SaveChangesAsync();TempData["SuccessMessage"]="Level approved successfully.";return RedirectToAction(nameof(Index));}

        [Authorize(Roles="Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id){var r=await _context.EmployeeLevels.FindAsync(id);if(r==null)return NotFound();r.Status="Rejected";await _context.SaveChangesAsync();TempData["SuccessMessage"]="Level request rejected.";return RedirectToAction(nameof(Index));}

        [Authorize(Roles="Employee")]
        public async Task<IActionResult> MyLevel(){var id=GetEmployeeId();if(id==null)return Forbid();var current=await _context.EmployeeLevels.Include(x=>x.Level).Where(x=>x.EmployeeId==id && x.Status=="Approved").OrderByDescending(x=>x.ApprovedDate).FirstOrDefaultAsync();var pending=await _context.EmployeeLevels.Include(x=>x.Level).Where(x=>x.EmployeeId==id && x.Status=="Pending").OrderByDescending(x=>x.RequestedDate).ToListAsync();ViewBag.Pending=pending;return View(current);}

        [Authorize(Roles="Employee")]
        public async Task<IActionResult> Request(){var id=GetEmployeeId();if(id==null)return Forbid();ViewBag.Levels=await _context.Levels.OrderBy(x=>x.LevelId).ToListAsync();return View(new EmployeeLevel());}

        [Authorize(Roles="Employee")]
        [HttpPost,ValidateAntiForgeryToken]
        public async Task<IActionResult> Request(EmployeeLevel request){
            var id=GetEmployeeId();
            if(id==null)return Forbid();
            if(!await _context.Levels.AnyAsync(x=>x.LevelId==request.LevelId)) ModelState.AddModelError("LevelId","Please select a valid level.");
            var pendingExists=await _context.EmployeeLevels.AnyAsync(x=>x.EmployeeId==id.Value && x.Status=="Pending");
            if(pendingExists) ModelState.AddModelError(string.Empty,"You already have a pending level request.");
            var current=await _context.EmployeeLevels.Where(x=>x.EmployeeId==id.Value && x.Status=="Approved").OrderByDescending(x=>x.ApprovedDate).FirstOrDefaultAsync();
            if(current!=null && current.LevelId==request.LevelId) ModelState.AddModelError("LevelId","This is already your approved level.");
            if(ModelState.IsValid){request.EmployeeId=id.Value;request.Status="Pending";request.RequestedDate=DateTime.Now;_context.EmployeeLevels.Add(request);await _context.SaveChangesAsync();TempData["SuccessMessage"]="Level request submitted for approval.";return RedirectToAction(nameof(MyLevel));}
            ViewBag.Levels=await _context.Levels.OrderBy(x=>x.LevelId).ToListAsync();return View(request);
        }
        private int? GetEmployeeId()=>int.TryParse(User.FindFirst("EmployeeId")?.Value,out var id)?id:null;
    }
}
