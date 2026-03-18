namespace PetCenterClient.DTOs
{
    public class OrderDetailRequestDTO
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public Guid? ImportStockDetailId { get; set; } // Cho phép null theo SQL
    }

    public class OrderDetailResponseDTO
    {
        public Guid OrderDetailId { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public Guid? ImportStockDetailId { get; set; }
    }
}
