using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EMS.Models
{
    public class EmployeeProfileViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, Phone, StringLength(20)]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Address { get; set; }

        [Required, StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? PhotoFile { get; set; }
        public string? PhotoPath { get; set; }
        public string? DepartmentName { get; set; }
        public string? DesignationTitle { get; set; }
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
    }
}
