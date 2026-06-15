using System.Text.Json.Serialization;

namespace PetCenterClient.ViewModels.Login
{
    public class GoogleCallbackViewModel
    {
        public string Code { get; set; } = null!;
        public string RedirectUri { get; set; } = null!;
    }

    public class GoogleLoginResponseViewModel
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("errorType")]
        public string? ErrorType { get; set; }

        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }

    public class GoogleClientViewModel
    {
        public string ClientId { get; set; } = null!;
    }
}
