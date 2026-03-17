using Google.Apis.Auth;
using IdentityAPI.Models;

namespace IdentityAPI.Service.Interface
{
    public interface IGoogleAuthService
    {
        /// <summary>
        /// Verify idToken với Google, trả về payload chứa email + name
        /// </summary>
        Task<GoogleJsonWebSignature.Payload> VerifyGoogleTokenAsync(string idToken);

        Task<string> ExchangeCodeForIdTokenAsync(string code, string redirectUri);

        Task<IdentityAPI.Models.Customer> GetOrCreateUserFromGoogleAsync(string email, string name);
    }
}

