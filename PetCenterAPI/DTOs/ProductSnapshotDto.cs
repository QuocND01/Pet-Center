namespace PetCenterAPI.DTOs
{
    public class ProductSnapshotRequestDto
    {
        public List<Guid> ProductIds { get; set; } = new();
    }

    public class ProductSnapshotResponseDto
    {
        public Guid ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public string CategoryName { get; set; } = null!;

        public string BrandName { get; set; } = null!;
    }
}
