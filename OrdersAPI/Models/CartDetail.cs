namespace OrdersAPI.Models
{
    public partial class CartDetail
    {
        public Guid CartDetailId { get; set; }

        public Guid CartId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public virtual Cart Cart { get; set; } = null!;
    }
}
