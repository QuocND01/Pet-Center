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

        // GET ALL & GET BY ID (Như cũ)
        public async Task<IEnumerable<AddressResponseDTO>> GetAddressesAsync()
            => _mapper.Map<IEnumerable<AddressResponseDTO>>(await _repo.GetAllAsync());

        public async Task<AddressResponseDTO?> GetAddressByIdAsync(Guid id)
            => _mapper.Map<AddressResponseDTO>(await _repo.GetByIdAsync(id));

        // CREATE
        public async Task<AddressResponseDTO> CreateAddressAsync(AddressCreateDTO dto)
        {
            var address = _mapper.Map<Address>(dto);
            address.AddressId = Guid.NewGuid();
            await _repo.AddAsync(address);
            await _repo.SaveChangesAsync();
            return _mapper.Map<AddressResponseDTO>(address);
        }

        // UPDATE
        public async Task<bool> UpdateAddressAsync(Guid id, AddressCreateDTO dto)
        {
            var existingAddress = await _repo.GetByIdAsync(id);
            if (existingAddress == null) return false;

            // Copy dữ liệu từ DTO vào bản ghi đang có trong DB
            _mapper.Map(dto, existingAddress);

            _repo.Update(existingAddress);
            return await _repo.SaveChangesAsync();
        }

        // DELETE
        public async Task<bool> DeleteAddressAsync(Guid id)
        {
            var address = await _repo.GetByIdAsync(id);
            if (address == null) return false;

            _repo.Delete(address);
            return await _repo.SaveChangesAsync();
        }
    }
}
