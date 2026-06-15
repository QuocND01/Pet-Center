namespace PetCenterAPI.Service.Interface
{
    public interface IEmailService
    {
        // ============================================================
        // REGISTER — OTP
        // ============================================================

        /// <summary>
        /// Send a verification email containing the OTP code to the given address
        /// </summary>
        Task SendVerificationEmail(string toEmail, string code);
    }
}
