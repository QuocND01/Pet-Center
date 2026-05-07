namespace ImportAPI.DTOs
{
    public class ImportSnapshotDto
    {
        public Guid ProductSnapshotId { get; set; }

        public Guid ImportStockDetailsId { get; set; }

        public string ProductName { get; set; } = null!;

        public string ProductCategory { get; set; } = null!;

        public string ProductBrand { get; set; } = null!;
    }
}
