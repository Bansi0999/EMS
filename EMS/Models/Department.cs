using System.ComponentModel.DataAnnotations;

namespace EMS.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100)]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        public ICollection<Designation>? Designations { get; set; }
        public ICollection<Employee>? Employees { get; set; }
    }
}
