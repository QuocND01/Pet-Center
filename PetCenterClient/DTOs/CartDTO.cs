namespace PetCenterClient.DTOs
{
    public class CartResponseDTO
    {
        public Guid CartId { get; set; }
        public Guid CustomerId { get; set; }
        public List<CartDetailResponseDTO> CartDetails { get; set; } = new();
    }

    public class CartDetailResponseDTO
    {
        public Guid CartDetailId { get; set; }
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }

        // Enriched from ProductService (populated in client controller)
        public string? ProductName { get; set; }
        public decimal? ProductPrice { get; set; }
        public string? ImageUrl { get; set; }
        public int? StockQuantity { get; set; }
        public decimal SubTotal => (ProductPrice ?? 0) * Quantity;
    }

    public class AddToCartRequestDTO
    {
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartDetailRequestDTO
    {
        public int Quantity { get; set; }
    }
}