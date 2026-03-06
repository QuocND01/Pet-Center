using ProductAPI.Models;

namespace ProductAPI.DTOs
{
    public class ReadBrandDTOs
    {
        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = null!;

        public string? BrandLogo { get; set; }
    }

    public class UpdateBrandDTOs
    {

        public string BrandName { get; set; } = null!;

        public string? BrandLogo { get; set; }
    }

    public class CreateBrandDTOs
    {

        public string BrandName { get; set; } = null!;

        public string? BrandLogo { get; set; }
    }
}
