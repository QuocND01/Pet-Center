using VoucherAPI.DTOs.Request;
using VoucherAPI.DTOs.Response;
using VoucherAPI.Models;
using VoucherAPI.Repositories.Interfaces;
using VoucherAPI.Services.Intterfaces;

namespace VoucherAPI.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _repo;

        public VoucherService(IVoucherRepository repo)
        {
            _repo = repo;
        }

        // ── GET ALL ──────────────────────────────────────────────
        public async Task<IEnumerable<VoucherDto>> GetAllAsync()
        {
            var vouchers = await _repo.GetAllAsync();
            var result = new List<VoucherDto>();

            foreach (var v in vouchers)
            {
                var usedCount = await _repo.GetUsedCountAsync(v.VoucherId);
                result.Add(MapToDto(v, usedCount));
            }

            return result;
        }

        // ── GET BY ID ────────────────────────────────────────────
        public async Task<VoucherDto?> GetByIdAsync(Guid id)
        {
            var v = await _repo.GetByIdAsync(id);
            if (v == null) return null;

            var usedCount = await _repo.GetUsedCountAsync(v.VoucherId);
            return MapToDto(v, usedCount);
        }

        // ── GET BY CODE ──────────────────────────────────────────
        public async Task<VoucherDto?> GetByCodeAsync(string code)
        {
            var v = await _repo.GetByCodeAsync(code);
            if (v == null) return null;

            var usedCount = await _repo.GetUsedCountAsync(v.VoucherId);
            return MapToDto(v, usedCount);
        }

        // ── CREATE ───────────────────────────────────────────────
        public async Task<(bool Success, string Message, VoucherDto? Data)> CreateAsync(CreateVoucherDto dto)
        {
            // 1. Check trùng code
            var codeExists = await _repo.CodeExistsAsync(dto.Code);
            if (codeExists)
                return (false, $"Voucher code '{dto.Code.ToUpper()}' already exists.", null);

            // 2. Business rule: MaxDiscountAmount không được vượt quá MinOrderAmount
            if (dto.MaxDiscountAmount >= dto.MinOrderAmount && dto.MinOrderAmount > 0)
                return (false, "Max discount amount must be less than min order amount.", null);

            // 3. Business rule: Kiểm tra effective discount rate
            //    Nếu không có cap hoặc cap quá cao → cảnh báo nhưng vẫn cho tạo
            //    Nhưng nếu MaxDiscount / MinOrder > 80% thì từ chối (bảo vệ margin tối thiểu)
            if (dto.MinOrderAmount > 0)
            {
                var effectiveRate = (dto.MaxDiscountAmount / dto.MinOrderAmount) * 100;
                if (effectiveRate > 80)
                    return (false, $"Effective discount rate ({effectiveRate:F1}%) exceeds 80%. Please lower Max Discount Amount or raise Min Order Amount.", null);
            }

            // 4. Expiry phải trong tương lai
            if (dto.ExpiredDate.HasValue && dto.ExpiredDate.Value <= DateTime.UtcNow)
                return (false, "Expiry date must be in the future.", null);

            // 5. Map và tạo
            var entity = new Voucher
            {
                Code = dto.Code.ToUpper().Trim(),
                DiscountPercent = dto.DiscountPercent,
                Description = dto.Description,
                MinOrderAmount = dto.MinOrderAmount,
                MaxDiscountAmount = dto.MaxDiscountAmount,
                UseageLimit = dto.UseageLimit,
                ExpiredDate = dto.ExpiredDate.HasValue
                                    ? DateTime.SpecifyKind(dto.ExpiredDate.Value, DateTimeKind.Utc)
                                    : null,
                IsActive = true,
                CreateAt = DateTime.UtcNow
            };

            var created = await _repo.CreateAsync(entity);
            return (true, "Voucher created successfully.", MapToDto(created, 0));
        }

        // ── UPDATE ───────────────────────────────────────────────
        public async Task<(bool Success, string Message, VoucherDto? Data)> UpdateAsync(Guid id, UpdateVoucherDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return (false, "Voucher not found.", null);

            // 1. Check trùng code (bỏ qua chính nó)
            var codeExists = await _repo.CodeExistsAsync(dto.Code, excludeId: id);
            if (codeExists)
                return (false, $"Voucher code '{dto.Code.ToUpper()}' already exists.", null);

            // 2. Business rules — giống Create
            if (dto.MaxDiscountAmount >= dto.MinOrderAmount && dto.MinOrderAmount > 0)
                return (false, "Max discount amount must be less than min order amount.", null);

            if (dto.MinOrderAmount > 0)
            {
                var effectiveRate = (dto.MaxDiscountAmount / dto.MinOrderAmount) * 100;
                if (effectiveRate > 80)
                    return (false, $"Effective discount rate ({effectiveRate:F1}%) exceeds 80%.", null);
            }

            if (dto.ExpiredDate.HasValue && dto.ExpiredDate.Value <= DateTime.UtcNow)
                return (false, "Expiry date must be in the future.", null);

            // 3. Không cho phép giảm UsageLimit xuống dưới số đã dùng
            if (dto.UseageLimit.HasValue)
            {
                var usedCount = await _repo.GetUsedCountAsync(id);
                if (dto.UseageLimit.Value < usedCount)
                    return (false, $"Usage limit cannot be less than the number of times already used ({usedCount}).", null);
            }

            // 4. Cập nhật fields
            existing.Code = dto.Code.ToUpper().Trim();
            existing.DiscountPercent = dto.DiscountPercent;
            existing.Description = dto.Description;
            existing.MinOrderAmount = dto.MinOrderAmount;
            existing.MaxDiscountAmount = dto.MaxDiscountAmount;
            existing.UseageLimit = dto.UseageLimit;
            existing.ExpiredDate = dto.ExpiredDate.HasValue
                                           ? DateTime.SpecifyKind(dto.ExpiredDate.Value, DateTimeKind.Utc)
                                           : null;
            existing.IsActive = dto.IsActive;

            var updated = await _repo.UpdateAsync(existing);
            var usedCountFinal = await _repo.GetUsedCountAsync(id);
            return (true, "Voucher updated successfully.", MapToDto(updated, usedCountFinal));
        }

        // ── TOGGLE STATUS ────────────────────────────────────────
        public async Task<(bool Success, string Message)> ToggleStatusAsync(Guid id, bool isActive)
        {
            var voucher = await _repo.GetByIdAsync(id);
            if (voucher == null)
                return (false, "Voucher not found.");

            // Không cho activate lại voucher đã hết hạn
            if (isActive && voucher.ExpiredDate.HasValue && voucher.ExpiredDate.Value <= DateTime.UtcNow)
                return (false, "Cannot activate an expired voucher. Please update the expiry date first.");

            // Không cho activate nếu đã dùng hết usage limit
            if (isActive && voucher.UseageLimit.HasValue)
            {
                var usedCount = await _repo.GetUsedCountAsync(id);
                if (usedCount >= voucher.UseageLimit.Value)
                    return (false, "Cannot activate voucher that has reached its usage limit.");
            }

            voucher.IsActive = isActive;
            await _repo.UpdateAsync(voucher);
            return (true, isActive ? "Voucher activated." : "Voucher deactivated.");
        }

        // ── MAPPER ───────────────────────────────────────────────
        private static VoucherDto MapToDto(Voucher v, int usedCount) => new VoucherDto
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
