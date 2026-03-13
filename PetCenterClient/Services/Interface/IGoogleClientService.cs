using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IGoogleClientService  // ← thêm public
    {
        GoogleClientDto GetGoogleClientId();
    }
}
