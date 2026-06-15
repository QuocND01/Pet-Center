using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.Register
{
    public class ResendOtpRequestDTO
    {
        [Required] public string Email { get; set; } = null!;
    }
}
