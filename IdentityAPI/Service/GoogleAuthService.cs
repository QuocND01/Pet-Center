using System.Text.Json;
using Google.Apis.Auth;
using IdentityAPI.Models;
using IdentityAPI.Repository.Interface;
using IdentityAPI.Security;
using IdentityAPI.Service.Interface;
using Microsoft.Extensions.Options;

namespace IdentityAPI.Service
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly GoogleAuthSettings _settings;
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmailService _emailService;
        private readonly PasswordService _passwordService;
        private readonly HttpClient _httpClient;

        public GoogleAuthService(
            IOptions<GoogleAuthSettings> settings,
            ICustomerRepository customerRepository,
            IEmailService emailService,
            PasswordService passwordService,
            IHttpClientFactory httpClientFactory)
        {
            _settings = settings.Value;
            _customerRepository = customerRepository;
            _emailService = emailService;
            _passwordService = passwordService;
            _httpClient = httpClientFactory.CreateClient();
        }

        // ── Method cũ: verify id_token trực tiếp (giữ nguyên) ──
        public async Task<GoogleJsonWebSignature.Payload> VerifyGoogleTokenAsync(string idToken)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _settings.ClientId }
            };
            return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }

        // ── Method mới: exchange authorization code → id_token ──
        public async Task<string> ExchangeCodeForIdTokenAsync(string code, string redirectUri)
        {
            // Gọi Google Token Endpoint để exchange code lấy tokens
            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", _settings.ClientId),
                new KeyValuePair<string, string>("client_secret", _settings.ClientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            });

            var response = await _httpClient.PostAsync(
                "https://oauth2.googleapis.com/token", tokenRequest);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Token exchange failed: {content}");

            var tokenData = JsonDocument.Parse(content).RootElement;

            // Google trả về id_token trong response
            if (!tokenData.TryGetProperty("id_token", out var idTokenElement))
                throw new Exception("id_token not found in Google response");

            return idTokenElement.GetString()!;
        }

        // ── GetOrCreate giữ nguyên ──
        public async Task<Customer> GetOrCreateUserFromGoogleAsync(string email, string name)
        {
            var existing = await _customerRepository.GetByEmailAsync(email);
            if (existing != null)
                return existing;

            var tempPassword = _passwordService.GenerateTemporaryPassword();
            var hashed = BCrypt.Net.BCrypt.HashPassword(tempPassword);

            var newCustomer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = email,
                FullName = name,
                PasswordHash = hashed,
                IsActive = true,
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                Gender = "",
                PhoneNumber = "",
                BirthDay = null,
                VerificationCode = null,
                VerificationExpire = null,
                
            };

            try
            {
                await _customerRepository.AddAsync(newCustomer);
            }
            catch (Exception ex)
            {
                // In ra inner exception đầy đủ
                var inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                Console.WriteLine($"DB Error: {inner.Message}");
                throw;
            }

            await _emailService.SendWelcomeEmailAsync(email, name, tempPassword);
            return newCustomer;
        }
    }
}

