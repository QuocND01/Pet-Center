using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto dto);

        Task<LoginResponseDto?> StaffLoginAsync(LoginDto dto);
    }
}
