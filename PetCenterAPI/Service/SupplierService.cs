using AutoMapper;
using Azure.Core;
using PetCenterAPI.DTOs;
using PetCenterAPI.DTOs.Requests.Supplier;
using PetCenterAPI.DTOs.Responses.Supplier;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;


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
            // 1. Kiểm tra trùng thông tin Supplier
            var duplicate = await _repository.FindDuplicateAsync(
                dto.TaxId,
                dto.SupplierName,
                dto.SupplierEmail,
                dto.SupplierPhoneNumber,
                null);

            if (duplicate != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.TaxId) &&
                    duplicate.TaxId == dto.TaxId)
                {
                    throw new InvalidOperationException(
                        "TaxId is conflict with other supplier, please try again!");
                }

                if (duplicate.SupplierName == dto.SupplierName)
                {
                    throw new InvalidOperationException(
                        "Supplier name is conflict with other supplier, please try again!");
                }

                if (duplicate.SupplierEmail == dto.SupplierEmail)
                {
                    throw new InvalidOperationException(
                        "Supplier email is conflict with other supplier, please try again!");
                }

                if (duplicate.SupplierPhoneNumber == dto.SupplierPhoneNumber)
                {
                    throw new InvalidOperationException(
                        "Supplier phone number is conflict with other supplier, please try again!");
                }
            }

            // 2. Map DTO sang Entity
            var supplier = _mapper.Map<Supplier>(dto);

            // 3. Gán giá trị mặc định
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

            if (supplier == null)
            {
                return false;
            }

            // Check duplicate supplier information
            var duplicate = await _repository.FindDuplicateAsync(
                dto.TaxId,
                dto.SupplierName,
                dto.SupplierEmail,
                dto.SupplierPhoneNumber,
                id);

            if (duplicate != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.TaxId) &&
                    duplicate.TaxId == dto.TaxId)
                {
                    throw new InvalidOperationException(
                        "TaxId is conflict with other supplier, please try again!");
                }

                if (duplicate.SupplierName == dto.SupplierName)
                {
                    throw new InvalidOperationException(
                        "Supplier name is conflict with other supplier, please try again!");
                }

                if (duplicate.SupplierEmail == dto.SupplierEmail)
                {
                    throw new InvalidOperationException(
                        "Supplier email is conflict with other supplier, please try again!");
                }

                if (duplicate.SupplierPhoneNumber == dto.SupplierPhoneNumber)
                {
                    throw new InvalidOperationException(
                        "Supplier phone number is conflict with other supplier, please try again!");
                }
            }

            _mapper.Map(dto, supplier);

            _repository.Update(supplier);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var supplier = await _repository.GetByIdAsync(id);

            if (supplier == null)
                return false;

            bool isUsed = await _repository.IsUsedInImportStockAsync(id);
            if (isUsed)
            {
                throw new InvalidOperationException(
                    "This supplier cannot be deleted because it is being used in an import stock!");
            }

            supplier.IsActive = false;

            _repository.Update(supplier);
            await _repository.SaveChangesAsync();

            return true;
        }


    }
}
