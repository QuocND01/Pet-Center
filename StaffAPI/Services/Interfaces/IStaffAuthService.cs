namespace StaffAPI.Services.Interfaces
{
    public interface IStaffAuthService
    {
        Task<(bool Success, string? Token, string? ErrorType, string Message, List<string> Roles)> LoginAsync(
        string email, string password);
    }
}
