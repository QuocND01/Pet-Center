using Microsoft.Extensions.Options;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.Login;

namespace PetCenterClient.Services
{
    public class GoogleAPIClient : IGoogleAPIClient
    {
        private readonly GoogleClientViewModel _googleClientDto;

        public GoogleAPIClient(IOptions<GoogleClientViewModel> googleClientDto)
        {
            _googleClientDto = googleClientDto.Value;
        }

        public GoogleClientViewModel GetGoogleClientId()
        {
            return _googleClientDto;
        }
    }
}
