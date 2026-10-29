using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EMS.Models
{
    public class ProfileViewModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, Phone, StringLength(20)]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        public string? PhotoPath { get; set; }

        // Read-only employee details shown on the profile page
        public string EmployeeCode { get; set; } = string.Empty;
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public DateTime? HireDate { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string DesignationTitle { get; set; } = string.Empty;

        [Display(Name = "Profile Picture")]
        public IFormFile? PhotoFile { get; set; }
    }
}
