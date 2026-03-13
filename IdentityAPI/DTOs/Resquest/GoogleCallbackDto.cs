namespace IdentityAPI.DTOs.Resquest
{
    public class GoogleCallbackDto
    {
        public string Code { get; set; } = null!;
        public string RedirectUri { get; set; } = null!;
    }
}
