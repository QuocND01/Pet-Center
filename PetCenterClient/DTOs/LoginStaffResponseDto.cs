namespace PetCenterClient.DTOs
{
    public class LoginStaffResponseDto
    {
        public bool Success { get; set; }
        public string? message { get; set; }
        public string? token { get; set; }
        public string? ErrorType { get; set; }
        public List<string>? roles { get; set; }
        public string? primaryRole { get; set; }
    }
}
