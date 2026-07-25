using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.Login
{
    public class ResetPasswordRequestDTO
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [RegularExpression(@"^(?=[^a-z]*[A-Z])(?=\S+$)(?=.*[@])(?=.*[0-9]).{6,}$",
    ErrorMessage = "Password must start with uppercase, contain @, a number, and no spaces")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
