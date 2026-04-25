namespace CustomerAPI.DTOs.Response
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public string? ErrorType { get; set; }
    }
}
