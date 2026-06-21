using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.DTOs.Requests.CustomerProfile.AddressRequestDTO;

namespace PetCenterAPI.Service
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;

        public AddressService(IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<List<ReadAddressDTO>> GetCustomerAddressesAsync(Guid customerId)
        {
            var addresses = await _addressRepository.GetAddressesByCustomerIdAsync(customerId);

            return addresses.Select(a => new ReadAddressDTO
            {
                AddressId = a.AddressId,
                Province = a.Province,
                District = a.District,
                Ward = a.Ward,
                AddressDetails = a.AddressDetails,
                IsDefault = a.IsDefault ?? false,
                IsActive = a.IsActive ?? true
            }).ToList();
        }

        public async Task<bool> AddAddressAsync(Guid customerId, MutateAddressDTO dto)
        {
            if (dto.IsDefault)
            {
                await _addressRepository.ResetDefaultAddressAsync(customerId);
            }

            var address = new Address
            {
                AddressId = Guid.NewGuid(),
                CustomerId = customerId,
                Province = dto.Province,
                District = dto.District,
                Ward = dto.Ward,
                AddressDetails = dto.AddressDetails,
                IsDefault = dto.IsDefault,
                IsActive = true // Mặc định địa chỉ mới tạo sẽ active
            };

            await _addressRepository.AddAddressAsync(address);
            await _addressRepository.SaveAsync();

            return true;
        }

        public async Task<bool> UpdateAddressAsync(Guid customerId, Guid addressId, MutateAddressDTO dto)
        {
            var address = await _addressRepository.GetAddressByIdAsync(addressId, customerId);
            if (address == null) return false;

            if (dto.IsDefault && address.IsDefault != true)
            {
                await _addressRepository.ResetDefaultAddressAsync(customerId);
            }

            address.Province = dto.Province;
            address.District = dto.District;
            address.Ward = dto.Ward;
            address.AddressDetails = dto.AddressDetails;
            address.IsDefault = dto.IsDefault;

            await _addressRepository.UpdateAddressAsync(address);
            await _addressRepository.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteAddressAsync(Guid customerId, Guid addressId)
        {
            var address = await _addressRepository.GetAddressByIdAsync(addressId, customerId);
            if (address == null) return false;

            await _addressRepository.DeleteAddressAsync(address);
            await _addressRepository.SaveAsync();

            return true;
        }
    }
}