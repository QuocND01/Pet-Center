using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.Login
{
    public class ForgotPasswordRequestDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }
}
