namespace PetCenterClient.DTOs
{
    public class GoogleCallbackRequestDto
    {
        public string Code { get; set; } = null!;
        public string RedirectUri { get; set; } = null!;
    }
}
