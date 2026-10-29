using System.ComponentModel.DataAnnotations;

namespace EMS.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required"), DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required"), StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password"), Compare(nameof(NewPassword), ErrorMessage = "New password and confirmation do not match."), DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
