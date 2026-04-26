using CustomerAPI.Models;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace CustomerAPI.Service.Interface
{
    public interface IGoogleAuthService
    {
        Task<GoogleJsonWebSignature.Payload> VerifyGoogleTokenAsync(string idToken);
        Task<string> ExchangeCodeForIdTokenAsync(string code, string redirectUri);
        Task<Customer> GetOrCreateUserFromGoogleAsync(string email, string name);
    }
}
