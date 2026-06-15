namespace PetCenterAPI.DTOs.Responses.Login
{
    public class LoginResponseDTO
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? ErrorType { get; set; }
        public string? Message { get; set; }
    }
}
