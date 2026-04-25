using System.ComponentModel.DataAnnotations;

namespace CustomerAPI.DTOs.Request
{
    public class VerifyOtpDto
    {
        [Required] public string Email { get; set; } = null!;
        [Required] public string Code { get; set; } = null!;
    }
}
