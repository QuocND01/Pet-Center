namespace PetCenterAPI.Security
{
    public interface IJwtService
    {
        // ============================================================
        // LOGIN
        // ============================================================

        /// <summary>
        /// Generate a signed JWT token containing user identity and roles
        /// </summary>
        string GenerateToken(Guid userId, string email, List<string> roles, string fullName = "");
    }
}
