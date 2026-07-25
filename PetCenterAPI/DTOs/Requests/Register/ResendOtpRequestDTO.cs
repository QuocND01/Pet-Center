using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.Register
{
    public class ResendOtpRequestDTO
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = null!;
    }
}
