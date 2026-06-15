namespace PetCenterAPI.Service.Interface
{
    public interface ICustomerAuthService
    {
        // ============================================================
        // LOGIN
        // ============================================================

        /// <summary>
        /// Validate credentials and return JWT token if login is successful
        /// </summary>
        Task<(bool success, string? token, string? errorType, string message)> LoginAsync(
            string email, string password);
    }
}
