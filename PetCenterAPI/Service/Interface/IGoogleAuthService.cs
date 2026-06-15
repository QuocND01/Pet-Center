using Google.Apis.Auth;
using PetCenterAPI.Models;

namespace PetCenterAPI.Service.Interface
{
    public interface IGoogleAuthService
    {
        // ============================================================
        // GOOGLE LOGIN
        // ============================================================
        Task<GoogleJsonWebSignature.Payload> VerifyGoogleTokenAsync(string idToken);
        Task<string> ExchangeCodeForIdTokenAsync(string code, string redirectUri);
        Task<Customer> GetOrCreateUserFromGoogleAsync(string email, string name);
    }
}
