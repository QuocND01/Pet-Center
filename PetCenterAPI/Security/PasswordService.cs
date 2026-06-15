namespace PetCenterAPI.Security
{
    public class PasswordService
    {
        // ============================================================
        // LOGIN
        // ============================================================

        /// <summary>
        /// Hash a plain-text password using BCrypt
        /// </summary>
        public string Hash(string password)
            => BCrypt.Net.BCrypt.HashPassword(password);

        /// <summary>
        /// Verify a plain-text password against a BCrypt hash
        /// </summary>
        public bool Verify(string password, string hash)
            => BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
