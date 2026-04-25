using AutoMapper;
using ImportAPI.DTOs;
using ImportAPI.Repository.Interface;
using ImportAPI.Service.Interface;
using ImportAPI.Models;


namespace ImportAPI.Service
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _repository;
        private readonly IMapper _mapper;

        public SupplierService(ISupplierRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReadSupplierDto>> GetAllAsync()
        {
            var suppliers = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ReadSupplierDto>>(suppliers);
        }

        public async Task<ReadSupplierDto?> GetByIdAsync(Guid id)
        {
            var supplier = await _repository.GetByIdAsync(id);
            if (supplier == null) return null;

            return _mapper.Map<ReadSupplierDto>(supplier);
        }

        public async Task<ReadSupplierDto> CreateAsync(CreateSupplierDto dto)
        {
            var supplier = _mapper.Map<Supplier>(dto);

            supplier.SupplierId = Guid.NewGuid();
            supplier.IsActive = true;

            await _repository.AddAsync(supplier);
            await _repository.SaveChangesAsync();

            return _mapper.Map<ReadSupplierDto>(supplier);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateSupplierDto dto)
        {
            var supplier = await _repository.GetByIdAsync(id);
            if (supplier == null) return false;

            _mapper.Map(dto, supplier);

            _repository.Update(supplier);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var supplier = await _repository.GetByIdAsync(id);
            if (supplier == null) return false;

            supplier.IsActive = false;

            _repository.Update(supplier);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
