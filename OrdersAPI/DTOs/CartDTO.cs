namespace OrdersAPI.DTOs
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