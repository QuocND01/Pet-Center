using System.ComponentModel.DataAnnotations;

namespace PetCenterClient.ViewModels.ManageVoucher
{
    public class VoucherViewModel
    {
        public Guid VoucherId { get; set; }
        public string Code { get; set; } = null!;
        public int? DiscountPercent { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public int? UseageLimit { get; set; }
        public int UsedCount { get; set; }
        public DateTime? CreateAt { get; set; }
        public string? Description { get; set; }
    }

    public class CreateVoucherViewModel
    {
        public string Code { get; set; } = null!;
        public int DiscountPercent { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        [Range(1, 999_999_999, ErrorMessage = "Min order amount must be between 1₫ and 999,999,999₫.")]
        public decimal MinOrderAmount { get; set; }

        [Range(1, 50_000_000, ErrorMessage = "Max discount must be between 1₫ and 50,000,000₫.")]
        public decimal MaxDiscountAmount { get; set; }

        [Range(1, 500, ErrorMessage = "Usage limit must be between 1 and 500.")]
        public int? UseageLimit { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
