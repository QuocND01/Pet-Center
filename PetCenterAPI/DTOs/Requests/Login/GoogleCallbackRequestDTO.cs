namespace PetCenterAPI.DTOs.Requests.Login
{
    public class GoogleCallbackRequestDTO
    {
        public string Code { get; set; } = null!;
        public string RedirectUri { get; set; } = null!;
    }
}
