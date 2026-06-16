using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.Login
{
    public class ForgotPasswordRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
