namespace PetCenterAPI.Service.Interface
{
    public interface IStaffAuthService
    {
        // ============================================================
        // LOGIN
        // ============================================================

        /// <summary>
        /// Validate staff credentials, check active status and roles,
        /// return JWT token and role list on success
        /// </summary>
        Task<(bool Success, string? Token, string? ErrorType, string Message, List<string> Roles)> LoginAsync(
            string email, string password);
    }
}
