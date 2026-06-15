using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PetCenterClient.ViewModels.Login
{
    public class StaffLoginViewModel
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }

    public class StaffLoginResponseViewModel
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("errorType")]
        public string? ErrorType { get; set; }

        [JsonPropertyName("roles")]
        public List<string>? Roles { get; set; }

        [JsonPropertyName("primaryRole")]
        public string? PrimaryRole { get; set; }
    }
}
