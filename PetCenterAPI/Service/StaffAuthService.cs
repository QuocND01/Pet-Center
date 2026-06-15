using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Security;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class StaffAuthService : IStaffAuthService
    {
        private readonly IStaffAuthRepository _staffAuthRepository;
        private readonly PasswordService _passwordService;
        private readonly IJwtService _jwtService;

        public StaffAuthService(
            IStaffAuthRepository staffAuthRepository,
            PasswordService passwordService,
            IJwtService jwtService)
        {
            _staffAuthRepository = staffAuthRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        // ============================================================
        // LOGIN
        // ============================================================

        /// <summary>
        /// Check email, password, active status and role assignment,
        /// then generate JWT token containing all assigned roles
        /// </summary>
        public async Task<(bool Success, string? Token, string? ErrorType, string Message, List<string> Roles)> LoginAsync(
            string email, string password)
        {
            var staff = await _staffAuthRepository.GetByEmailAsync(email);

            if (staff == null)
                return (false, null, "InvalidCredentials", "Email or password incorrect", new());

            if (!staff.IsActive)
                return (false, null, "AccountInactive",
                    "Your account has been deactivated. Please contact admin.", new());

            if (!_passwordService.Verify(password, staff.PasswordHash))
                return (false, null, "InvalidCredentials", "Email or password incorrect", new());

            var roles = staff.Roles
                .Where(r => r.IsActive)
                .Select(r => r.RoleName)
                .ToList();

            if (!roles.Any())
                return (false, null, "NoPermission",
                    "This account does not have permission to access the system.", new());

            var token = _jwtService.GenerateToken(
                staff.StaffId,
                staff.Email,
                new List<string>(roles),
                staff.FullName);

            return (true, token, null, "Login success", roles);
        }
    }
}
