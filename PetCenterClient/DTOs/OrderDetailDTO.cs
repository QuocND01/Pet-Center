namespace PetCenterClient.DTOs
{
    public class OrderDetailRequestDTO
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? ImportStockDetailId { get; set; } // Cho phép null theo SQL
    }

    public class OrderDetailResponseDTO
    {
        public Guid OrderDetailId { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? ImportStockDetailId { get; set; }
    }
    //Mapping format: "productId:quantity,productId:quantity" for both deduct and return stock
    public class DeductStockResponse
    {
        public string? Mapping { get; set; }
    }
}
