using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.Register
{
    public class VerifyOtpRequestDTO
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Code is required")]
        public string Code { get; set; } = null!;
    }
}
