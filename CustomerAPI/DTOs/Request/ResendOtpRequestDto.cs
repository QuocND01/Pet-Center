using System.ComponentModel.DataAnnotations;

namespace CustomerAPI.DTOs.Request
{
    public class ResendOtpDto
    {
        [Required] public string Email { get; set; } = null!;
    }
}
