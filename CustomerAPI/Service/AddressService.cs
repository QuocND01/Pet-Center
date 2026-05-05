using AutoMapper;
using CustomerAPI.DTOs.Address;
using CustomerAPI.Models;
using CustomerAPI.Repository.Interface;
using CustomerAPI.Service.Interface;

namespace CustomerAPI.Service
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _repo;
        private readonly IMapper _mapper;

        public AddressService(IAddressRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<ViewAddressDto>> GetMyAddressesAsync(Guid customerId)
        {
            var data = await _repo.GetByCustomerIdAsync(customerId);
            return _mapper.Map<List<ViewAddressDto>>(data);
        }

        public async Task<ViewAddressDto?> GetByIdAsync(Guid id)
        {
            var address = await _repo.GetByIdAsync(id);
            return address == null ? null : _mapper.Map<ViewAddressDto>(address);
        }

        public async Task<ViewAddressDto> CreateAsync(Guid customerId, CreateAddressDto dto)
        {
            var address = _mapper.Map<Address>(dto);
            address.CustomerId = customerId;
            address.IsActive = true;

            // nếu là default → set các cái khác false
            if (dto.IsDefault == true)
            {
                var list = await _repo.GetByCustomerIdAsync(customerId);
                foreach (var item in list)
                    item.IsDefault = false;
            }

            await _repo.AddAsync(address);
            await _repo.SaveChangesAsync();

            return _mapper.Map<ViewAddressDto>(address);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateAddressDto dto)
        {
            var address = await _repo.GetByIdAsync(id);
            if (address == null) return false;

            _mapper.Map(dto, address);

            _repo.Update(address);
            await _repo.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var address = await _repo.GetByIdAsync(id);
            if (address == null) return false;

            _repo.Delete(address);
            await _repo.SaveChangesAsync();

            return true;
        }
    }
}
