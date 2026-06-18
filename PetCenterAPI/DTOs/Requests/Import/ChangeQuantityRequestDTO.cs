namespace PetCenterAPI.DTOs.Requests.Import
{
    public class IncreaseStockRequestDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
    public class DecreaseStockRequestDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
