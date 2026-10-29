using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMS.Models
{
    public class EmployeeLevel
    {
        public int EmployeeLevelId { get; set; }
        [Required] public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")] public Employee? Employee { get; set; }
        [Required] public int LevelId { get; set; }
        [ForeignKey("LevelId")] public Level? Level { get; set; }
        [Required, StringLength(20)] public string Status { get; set; } = "Pending";
        [StringLength(500)] public string? Remarks { get; set; }
        public DateTime RequestedDate { get; set; } = DateTime.Now;
        public DateTime? ApprovedDate { get; set; }
    }
}
