using PetCenterClient.DTOs;
using PetCenterClient.ViewModels.Login;

namespace PetCenterClient.Services.Interface
{
    public interface IGoogleAPIClient 
    {
        GoogleClientViewModel GetGoogleClientId();
    }
}
