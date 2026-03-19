namespace PromotionAPI.DTOs
{
    public class VoucherResponseDTO
    {
        public Guid VoucherId { get; set; }
        public string Code { get; set; }
        public int? DiscountPercent { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public decimal? MinOrderAmount { get; set; }
    }
}
