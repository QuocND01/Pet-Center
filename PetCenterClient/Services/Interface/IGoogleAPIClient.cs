using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IGoogleAPIClient  // ← thêm public
    {
        GoogleClientDto GetGoogleClientId();
    }
}
