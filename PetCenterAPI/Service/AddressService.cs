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
            // If customer has no active addresses, make this new address the default
            var existing = await _addressRepository.GetAddressesByCustomerIdAsync(customerId);
            var hasActive = existing != null && existing.Any();

            // Prevent duplicate addresses for the same customer (compare normalized fields)
            static string Normalize(string? s) => (s ?? string.Empty).Trim().ToLowerInvariant();
            bool IsDuplicate(Address a, MutateAddressDTO d)
            {
                return Normalize(a.Province) == Normalize(d.Province)
                    && Normalize(a.District) == Normalize(d.District)
                    && Normalize(a.Ward) == Normalize(d.Ward)
                    && Normalize(a.AddressDetails) == Normalize(d.AddressDetails);
            }

            if (existing != null && existing.Any(a => IsDuplicate(a, dto)))
            {
                // Duplicate found among active addresses - reject add
                return false;
            }

            bool isDefaultFinal = dto.IsDefault;
            if (!hasActive)
            {
                isDefaultFinal = true;
            }
            else if (dto.IsDefault)
            {
                // If explicitly marked as default, reset other defaults
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
                IsDefault = isDefaultFinal,
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

            // Prevent updating to a duplicate of another active address
            var existing = await _addressRepository.GetAddressesByCustomerIdAsync(customerId);
            if (existing != null && existing.Any())
            {
                static string Normalize(string? s) => (s ?? string.Empty).Trim().ToLowerInvariant();
                bool IsDuplicate(Address a, MutateAddressDTO d)
                {
                    return Normalize(a.Province) == Normalize(d.Province)
                        && Normalize(a.District) == Normalize(d.District)
                        && Normalize(a.Ward) == Normalize(d.Ward)
                        && Normalize(a.AddressDetails) == Normalize(d.AddressDetails);
                }

                if (existing.Any(a => a.AddressId != addressId && IsDuplicate(a, dto)))
                {
                    // Duplicate would be created - reject update
                    return false;
                }
            }

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

            // Do not allow deleting the default address
            if (address.IsDefault == true)
            {
                return false;
            }

            // Perform soft-delete via repository
            await _addressRepository.DeleteAddressAsync(address);
            await _addressRepository.SaveAsync();

            return true;
        }
    }
}