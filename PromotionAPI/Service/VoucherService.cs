using AutoMapper;
using PromotionAPI.DTOs;
using PromotionAPI.Models;
using PromotionAPI.Repository.Interface;
using PromotionAPI.Service.Interface;

namespace PromotionAPI.Service
{
    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _repo;
        private readonly IMapper _mapper;

        public VoucherService(IVoucherRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<VoucherResponseDTO>> GetAllAsync()
            => _mapper.Map<List<VoucherResponseDTO>>(await _repo.GetAllAsync());

        public async Task<VoucherResponseDTO?> GetByIdAsync(Guid id)
            => _mapper.Map<VoucherResponseDTO>(await _repo.GetByIdAsync(id));

        public async Task CreateAsync(CreateVoucherDTO dto)
        {
            var v = _mapper.Map<Voucher>(dto);

            v.VoucherId = Guid.NewGuid();
            v.CreateAt = DateTime.Now;
            v.IsActive = true;

            await _repo.AddAsync(v);
        }

        public async Task DeleteAsync(Guid id)
            => await _repo.DeleteAsync(id);

        public async Task<object> ApplyVoucherAsync(ApplyVoucherDTO dto)
        {
            var voucher = await _repo.GetByCodeAsync(dto.Code);

            if (voucher == null)
                return new { success = false, message = "Voucher not found" };

            if (voucher.IsActive != true)
                return new { success = false, message = "Voucher inactive" };

            if (voucher.ExpiredDate < DateTime.Now)
                return new { success = false, message = "Voucher expired" };

            if (dto.OrderAmount < voucher.MinOrderAmount)
                return new { success = false, message = "Not enough order value" };

            var totalUsed = await _repo.CountVoucherUsage(voucher.VoucherId);
            if (voucher.UseageLimit.HasValue && totalUsed >= voucher.UseageLimit)
                return new { success = false, message = "Voucher usage limit reached" };

            var used = await _repo.GetCustomerVoucher(dto.CustomerId, voucher.VoucherId);
            if (used != null && used.IsUsed == true)
                return new { success = false, message = "Voucher already used" };

            var discount = dto.OrderAmount * (voucher.DiscountPercent ?? 0) / 100;

            if (voucher.MaxDiscountAmount.HasValue && discount > voucher.MaxDiscountAmount)
                discount = voucher.MaxDiscountAmount.Value;

            return new
            {
                success = true,
                discountAmount = discount,
                finalAmount = dto.OrderAmount - discount,
                voucherId = voucher.VoucherId
            };
        }

        public async Task UpdateAsync(Guid id, CreateVoucherDTO dto)
        {
            var v = await _repo.GetByIdAsync(id);
            if (v == null) return;

            v.Code = dto.Code;
            v.DiscountPercent = dto.DiscountPercent;
            v.ExpiredDate = dto.ExpiredDate;
            v.MinOrderAmount = dto.MinOrderAmount;
            v.MaxDiscountAmount = dto.MaxDiscountAmount;
            v.UseageLimit = dto.UseageLimit;
            v.Description = dto.Description;

            await _repo.UpdateAsync(v);
        }

        public async Task<List<VoucherResponseDTO>> SearchAsync(string? code)
        {
            var data = await _repo.SearchAsync(code);
            return _mapper.Map<List<VoucherResponseDTO>>(data);
        }
    }

}
