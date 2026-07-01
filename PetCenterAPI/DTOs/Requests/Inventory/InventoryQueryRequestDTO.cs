namespace PetCenterAPI.DTOs.Requests.Inventory
{
    public class InventoryQueryRequestDTO
    {
        public string? Keyword { get; set; }

        public Guid? CategoryId { get; set; }

        public Guid? BrandId { get; set; }

        public bool? LowStock { get; set; }

        public bool? OutOfStock { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
