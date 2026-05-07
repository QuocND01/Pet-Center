using Microsoft.Extensions.Options;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Services
{
    public class GoogleAPIClient : IGoogleClientService
    {
        private readonly GoogleClientDto _googleClientDto;

        public GoogleAPIClient(IOptions<GoogleClientDto> googleClientDto)
        {
            _googleClientDto = googleClientDto.Value;
        }

        public GoogleClientDto GetGoogleClientId()
        {
            return _googleClientDto;
        }
    }
}
