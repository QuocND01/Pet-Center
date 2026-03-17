namespace PetCenterClient.DTOs
{
    public class LoginResponseDto
    {
        public string message { get; set; }
        public string token { get; set; }

        public bool Success { get; set; }

        public string ErrorType { get; set; }
    }
}
