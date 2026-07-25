using AutoMapper;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using PetCenterAPI.Models;
using PetCenterAPI.DTOs;
using PetCenterAPI.DTOs.Responses.Supplier;
using PetCenterAPI.DTOs.Requests.Supplier;


namespace PetCenterAPI.Service
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

        public async Task<IEnumerable<ReadSupplierResponseDTO>> GetAllAsync()
        {
            var suppliers = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ReadSupplierResponseDTO>>(suppliers);
        }

        public async Task<ReadSupplierResponseDTO?> GetByIdAsync(Guid id)
        {
            var supplier = await _repository.GetByIdAsync(id);
            if (supplier == null) return null;

            return _mapper.Map<ReadSupplierResponseDTO>(supplier);
        }


        public async Task<ReadSupplierResponseDTO> CreateAsync(CreateSupplierRequestDTO dto)
        {
            // 1. Kiểm tra trùng TaxId (chỉ check khi TaxId không rỗng/null)
            if (!string.IsNullOrWhiteSpace(dto.TaxId))
            {
                bool isTaxIdExist = await _repository.GetByTaxIdAsync(dto.TaxId);
                if (isTaxIdExist)
                {
                    // Nên ném Custom Exception hoặc InvalidOperationException để API trả về StatusCode 409/400 phù hợp
                    throw new InvalidOperationException("TaxId is conflict with other supplier, please try again!");
                }
            }

            // 2. Map DTO sang Entity
            var supplier = _mapper.Map<Supplier>(dto);

            // 3. Gán các giá trị mặc định
            supplier.SupplierId = Guid.NewGuid();
            supplier.IsActive = true;

            // 4. Lưu vào Database
            await _repository.AddAsync(supplier);
            await _repository.SaveChangesAsync();

            // 5. Trả về Response DTO
            return _mapper.Map<ReadSupplierResponseDTO>(supplier);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateSupplierRequestDTO dto)
        {
            var supplier = await _repository.GetByIdAsync(id);
            if (!string.IsNullOrWhiteSpace(dto.TaxId))
            {
                bool isTaxIdExist = await _repository.GetByTaxIdAsync(dto.TaxId);
                if (isTaxIdExist)
                {
                    // Nên ném Custom Exception hoặc InvalidOperationException để API trả về StatusCode 409/400 phù hợp
                    throw new InvalidOperationException("TaxId is conflict with other supplier, please try again!");
                }
            }

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
