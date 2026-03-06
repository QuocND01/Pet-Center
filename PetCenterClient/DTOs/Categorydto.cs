using PetCenterClient.Models;

namespace PetCenterClient.DTOs
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
