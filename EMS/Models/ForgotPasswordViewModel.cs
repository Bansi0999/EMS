using System.ComponentModel.DataAnnotations;
namespace EMS.Models
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    }
    public class ResetPasswordViewModel
    {
        [Required] [Display(Name = "Email address")] [EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, StringLength(10, MinimumLength = 6)] [Display(Name = "Email OTP")] public string Otp { get; set; } = string.Empty;
        [Required, StringLength(100, MinimumLength = 8)] [DataType(DataType.Password)] public string NewPassword { get; set; } = string.Empty;
        [Required, Compare(nameof(NewPassword))] [DataType(DataType.Password)] [Display(Name = "Confirm password")] public string ConfirmPassword { get; set; } = string.Empty;
    }
}
