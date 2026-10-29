using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMS.Models
{
    public class Designation
    {
        public int DesignationId { get; set; }

        [Required(ErrorMessage = "Designation title is required")]
        [StringLength(100)]
        [Display(Name = "Designation Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public ICollection<Employee>? Employees { get; set; }
    }
}
