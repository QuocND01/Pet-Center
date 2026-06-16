using PetCenterAPI.DTOs.Responses.ManageVoucher;
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
