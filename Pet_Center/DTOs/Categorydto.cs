using ProductAPI.Models;

namespace ProductAPI.DTOs
{
    public class ReadCategoryDTOs
    {

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;

        public string? CategoryLogo { get; set; }

    }

    public class UpdateCategoryDTOs
    {
        public string CategoryName { get; set; } = null!;

        public string? CategoryLogo { get; set; }
    }

    public class CreateCategoryDTOs
    {
        public string CategoryName { get; set; } = null!;

        public string? CategoryLogo { get; set; }
    }
}
