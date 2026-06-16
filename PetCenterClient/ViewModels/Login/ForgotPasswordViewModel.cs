using System.Text.Json.Serialization;

namespace PetCenterClient.ViewModels.Login
{
    public class ForgotPasswordViewModel
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ValidateTokenResponseViewModel
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}
