using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMS.Models
{
    public class Salary
    {
        public int SalaryId { get; set; }
        [Required] public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")] public Employee? Employee { get; set; }
        [Required, Range(0, 100000000)] public decimal BasicSalary { get; set; }
        [Range(0, 100000000)] public decimal Allowances { get; set; }
        [Range(0, 100000000)] public decimal Deductions { get; set; }
        [NotMapped] public decimal NetSalary => BasicSalary + Allowances - Deductions;
        [Required, StringLength(20)] public string Month { get; set; } = DateTime.Now.ToString("yyyy-MM");
        [Required, StringLength(20)] public string Status { get; set; } = "Pending";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ApprovedDate { get; set; }
    }
}
