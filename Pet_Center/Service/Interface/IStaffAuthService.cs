namespace Pet_Center.Service.Interface
{
    public interface IStaffAuthService
    {
        Task<string?> LoginAsync(string email, string password);
    }
}
