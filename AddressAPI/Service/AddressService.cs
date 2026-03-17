using AddressAPI.Models;
using AddressAPI.Repository.Interface;
using AddressAPI.Service.Interface;
using AutoMapper;

namespace AddressAPI.Service
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

        public async Task<IEnumerable<AddressResponseDTO>> GetAddressesAsync()
            => _mapper.Map<IEnumerable<AddressResponseDTO>>(await _repo.GetAllAsync());

        public async Task<IEnumerable<AddressResponseDTO>> GetAddressesByCustomerIdAsync(Guid customerId)
            => _mapper.Map<IEnumerable<AddressResponseDTO>>(
                await _repo.GetByCustomerIdAsync(customerId));

        public async Task<AddressResponseDTO?> GetAddressByIdAsync(Guid id)
            => _mapper.Map<AddressResponseDTO>(await _repo.GetByIdAsync(id));

        public async Task<AddressResponseDTO> CreateAddressAsync(AddressCreateDTO dto)
        {
            var address = _mapper.Map<Address>(dto);
            address.AddressId = Guid.NewGuid();
            await _repo.AddAsync(address);
            await _repo.SaveChangesAsync();
            return _mapper.Map<AddressResponseDTO>(address);
        }

        public async Task<bool> UpdateAddressAsync(Guid id, AddressCreateDTO dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;
            _mapper.Map(dto, existing);
            _repo.Update(existing);
            return await _repo.SaveChangesAsync();
        }

        public async Task<bool> DeleteAddressAsync(Guid id)
        {
            var address = await _repo.GetByIdAsync(id);
            if (address == null) return false;
            _repo.Delete(address);
            return await _repo.SaveChangesAsync();
        }
    }
}