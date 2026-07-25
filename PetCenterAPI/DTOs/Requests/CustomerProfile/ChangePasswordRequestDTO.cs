using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.CustomerProfile
{
    public class ChangePasswordRequestDTO
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [RegularExpression(@"^(?=[^a-z]*[A-Z])(?=\S+$)(?=.*[@])(?=.*[0-9]).{6,}$",
        ErrorMessage = "Password must start with uppercase, contain @, a number, and no spaces")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm your new password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
