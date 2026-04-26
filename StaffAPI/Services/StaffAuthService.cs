using StaffAPI.Repositories.Interfaces;
using StaffAPI.Security;
using StaffAPI.Services.Interfaces;

namespace StaffAPI.Services
{
    public class StaffAuthService : IStaffAuthService
    {
        private readonly IStaffAuthRepository _staffRepository;
        private readonly PasswordService _passwordService;
        private readonly IJwtService _jwtService;

        public StaffAuthService(
            IStaffAuthRepository staffRepository,
            PasswordService passwordService,
            IJwtService jwtService)
        {
            _staffRepository = staffRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        public async Task<(bool Success, string? Token, string? ErrorType, string Message, List<string> Roles)> LoginAsync(
    string email, string password)
        {
            var staff = await _staffRepository.GetByEmailAsync(email);

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
                roles.Add("Staff");

            var token = _jwtService.GenerateToken(staff.StaffId, staff.Email, roles);

            return (true, token, null, "Login success", roles); 
        }
    }
}
