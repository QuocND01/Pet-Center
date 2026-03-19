namespace PetCenterClient.DTOs
{
    public class VoucherDTO
    {
        public Guid VoucherId { get; set; }
        public string Code { get; set; }
        public int? DiscountPercent { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public int? UseageLimit { get; set; }
        public string? Description { get; set; }
    }
}
