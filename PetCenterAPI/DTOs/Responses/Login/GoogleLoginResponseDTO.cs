namespace PetCenterAPI.DTOs.Responses.Login
{
    public class GoogleLoginResponseDTO
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public string? ErrorType { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }
}
