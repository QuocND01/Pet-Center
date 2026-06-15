using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.Register
{
    public class VerifyOtpRequestDTO
    {
        [Required] public string Email { get; set; } = null!;
        [Required] public string Code { get; set; } = null!;
    }
}
