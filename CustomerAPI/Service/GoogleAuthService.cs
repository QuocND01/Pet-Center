using CustomerAPI.Models;
using CustomerAPI.Repository.Interface;
using CustomerAPI.Security;
using CustomerAPI.Service.Interface;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CustomerAPI.Service
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

        public async Task<GoogleJsonWebSignature.Payload> VerifyGoogleTokenAsync(string idToken)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _settings.ClientId }
            };
            return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }

        public async Task<string> ExchangeCodeForIdTokenAsync(string code, string redirectUri)
        {
            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code",          code),
                new KeyValuePair<string, string>("client_id",     _settings.ClientId),
                new KeyValuePair<string, string>("client_secret", _settings.ClientSecret),
                new KeyValuePair<string, string>("redirect_uri",  redirectUri),
                new KeyValuePair<string, string>("grant_type",    "authorization_code")
            });

            var response = await _httpClient.PostAsync(
                "https://oauth2.googleapis.com/token", tokenRequest);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Token exchange failed: {content}");

            var tokenData = JsonDocument.Parse(content).RootElement;
            if (!tokenData.TryGetProperty("id_token", out var idTokenElement))
                throw new Exception("id_token not found in Google response");

            return idTokenElement.GetString()!;
        }

        public async Task<Customer> GetOrCreateUserFromGoogleAsync(string email, string name)
        {
            // GetByEmailAsync chỉ lấy active customer
            var existing = await _customerRepository.GetByEmailAsync(email);
            if (existing != null)
                return existing;

            var tempPassword = _passwordService.GenerateTemporaryPassword();

            var newCustomer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = email,
                FullName = name,
                PasswordHash = _passwordService.Hash(tempPassword),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Gender = "",
                PhoneNumber = "",
                BirthDay = null
            };

            try
            {
                await _customerRepository.AddAsync(newCustomer);
            }
            catch (Exception ex)
            {
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
