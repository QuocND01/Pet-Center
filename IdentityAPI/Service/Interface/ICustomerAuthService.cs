namespace IdentityAPI.Service.Interface
{
    public interface ICustomerAuthService
    {
        Task<(bool success, string token, string errorType, string message)> LoginAsync(string email, string password);
    }
}
