using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.Login
{
    public class StaffLoginRequestDTO
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
