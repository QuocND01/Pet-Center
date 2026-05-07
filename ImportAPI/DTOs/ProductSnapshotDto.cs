namespace ImportAPI.DTOs
{
    public class ProductSnapshotDto
    {
        public Guid ProductSnapshotId { get; set; }

        public Guid ImportStockDetailsId { get; set; }

        public string ProductName { get; set; } = null!;

        public string ProductCategory { get; set; } = null!;

        public string ProductBrand { get; set; } = null!;
    }
    public class ProductSnapshotResponseDto
    {
        public Guid ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public string CategoryName { get; set; } = null!;

        public string BrandName { get; set; } = null!;
    }
}
