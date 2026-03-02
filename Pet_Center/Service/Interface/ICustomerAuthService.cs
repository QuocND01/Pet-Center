namespace ProductAPI.Service.Interface
{
    public interface ICustomerAuthService
    {
        Task<string?> LoginAsync(string email, string password);
    }
}
