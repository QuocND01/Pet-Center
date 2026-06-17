using PetCenterAPI.DTOs.Requests.ManageVoucher;
using PetCenterAPI.DTOs.Responses.ManageVoucher;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _voucherRepository;

        public VoucherService(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        // ============================================================
        // VOUCHER — VIEW LIST
        // ============================================================
        public async Task<IEnumerable<VoucherResponseDTO>> GetAllAsync()
        {
            var vouchers = await _voucherRepository.GetAllAsync();
            var result = new List<VoucherResponseDTO>();

            foreach (var v in vouchers)
            {
                var usedCount = await _voucherRepository.GetUsedCountAsync(v.VoucherId);
                result.Add(MapToResponse(v, usedCount));
            }

            return result;
        }

        // ============================================================
        // VOUCHER — CREATE
        // ============================================================
        public async Task<(bool Success, string Message, VoucherResponseDTO? Data)> CreateAsync(CreateVoucherRequestDTO request)
        {
            var codeExists = await _voucherRepository.CodeExistsAsync(request.Code);
            if (codeExists)
                return (false, $"Voucher code '{request.Code.ToUpper()}' already exists.", null);

            if (request.MaxDiscountAmount >= request.MinOrderAmount && request.MinOrderAmount > 0)
                return (false, "Max discount amount must be less than min order amount.", null);

            if (request.MinOrderAmount > 0)
            {
                var effectiveRate = (request.MaxDiscountAmount / request.MinOrderAmount) * 100;
                if (effectiveRate > 80)
                    return (false,
                        $"Effective discount rate ({effectiveRate:F1}%) exceeds 80%. Please lower Max Discount Amount or raise Min Order Amount.",
                        null);
            }

            if (request.ExpiredDate.HasValue && request.ExpiredDate.Value <= DateTime.UtcNow)
                return (false, "Expiry date must be in the future.", null);

            var entity = new Voucher
            {
                Code = request.Code.ToUpper().Trim(),
                DiscountPercent = request.DiscountPercent,
                Description = request.Description,
                MinOrderAmount = request.MinOrderAmount,
                MaxDiscountAmount = request.MaxDiscountAmount,
                UseageLimit = request.UseageLimit,
                ExpiredDate = request.ExpiredDate.HasValue
                    ? DateTime.SpecifyKind(request.ExpiredDate.Value, DateTimeKind.Utc)
                    : null,
                IsActive = true,
                CreateAt = DateTime.UtcNow
            };

            var created = await _voucherRepository.CreateAsync(entity);

            return (true, "Voucher created successfully.", MapToResponse(created, 0));
        }

        // ============================================================
        // VOUCHER — GET BY ID
        // ============================================================
        public async Task<VoucherResponseDTO?> GetByIdAsync(Guid id)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id);
            if (voucher == null) return null;

            var usedCount = await _voucherRepository.GetUsedCountAsync(voucher.VoucherId);
            return MapToResponse(voucher, usedCount);
        }

        // ============================================================
        // VOUCHER — TOGGLE STATUS
        // ============================================================
        public async Task<(bool Success, string Message)> ToggleStatusAsync(Guid id, bool isActive)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id);
            if (voucher == null)
                return (false, "Voucher not found.");

            if (isActive && voucher.ExpiredDate.HasValue && voucher.ExpiredDate.Value <= DateTime.UtcNow)
                return (false, "Cannot activate an expired voucher. Please update the expiry date first.");

            if (isActive && voucher.UseageLimit.HasValue)
            {
                var usedCount = await _voucherRepository.GetUsedCountAsync(id);
                if (usedCount >= voucher.UseageLimit.Value)
                    return (false, "Cannot activate voucher that has reached its usage limit.");
            }

            voucher.IsActive = isActive;
            await _voucherRepository.UpdateAsync(voucher);

            return (true, isActive ? "Voucher activated." : "Voucher deactivated.");
        }

        // ============================================================
        // HELPER
        // ============================================================
        private static VoucherResponseDTO MapToResponse(Models.Voucher v, int usedCount)
        {
            return new VoucherResponseDTO
            {
                VoucherId = v.VoucherId,
                Code = v.Code,
                DiscountPercent = v.DiscountPercent,
                IsActive = v.IsActive,
                ExpiredDate = v.ExpiredDate,
                MinOrderAmount = v.MinOrderAmount,
                MaxDiscountAmount = v.MaxDiscountAmount,
                UseageLimit = v.UseageLimit,
                UsedCount = usedCount,
                CreateAt = v.CreateAt,
                Description = v.Description
            };
        }
    }
}
