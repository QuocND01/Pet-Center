namespace PetCenterAPI.DTOs.Responses.Login
{
    public class StaffLoginResponseDTO
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public string? ErrorType { get; set; }
        public List<string>? Roles { get; set; }
        public string? PrimaryRole { get; set; }
    }
}
