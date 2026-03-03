using IdentityAPI.Repository.Interface;
using IdentityAPI.Security;
using IdentityAPI.Service.Interface;

namespace IdentityAPI.Service
{
    public class StaffAuthService : IStaffAuthService
    {
        private readonly IStaffRepository _staffRepository;
        private readonly PasswordService _passwordService;
        private readonly IJwtService _jwtService;

        public StaffAuthService(
            IStaffRepository staffRepository,
            PasswordService passwordService,
            IJwtService jwtService)
        {
            _staffRepository = staffRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            var staff = await _staffRepository.GetByEmailAsync(email);

            if (staff == null)
                return null;

            if (staff.IsActive != true)
                return null;

            if (string.IsNullOrEmpty(staff.PasswordHash))
                return null;

            var isValid = _passwordService.Verify(password, staff.PasswordHash);

            if (!isValid)
                return null;

            var roles = staff.Roles
                .Select(r => r.RoleName)
                .ToList();

            return _jwtService.GenerateToken(staff.StaffId,staff.Email!, roles);
        }
    }
}
