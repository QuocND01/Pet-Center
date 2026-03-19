namespace PromotionAPI.DTOs
{
    public interface CreateVoucherDTO
    {
        public string Code { get; set; }
        public int DiscountPercent { get; set; }
        public DateTime ExpiredDate { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public int UseageLimit { get; set; }
        public string Description { get; set; }
    }
}
