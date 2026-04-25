using System.ComponentModel.DataAnnotations;

namespace CustomerAPI.DTOs.Response
{
    public class ChangePasswordResponseDto
    {
        [Required]
        public string CurrentPassword { get; set; } = null!;

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = null!;

        [Required, Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
