namespace PromotionAPI.DTOs
{
    public interface ApplyVoucherDTO
    {
        public Guid CustomerId { get; set; }
        public string Code { get; set; }
        public decimal OrderAmount { get; set; }
    }
}
