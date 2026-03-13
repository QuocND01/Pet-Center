using ProductAPI.DTOs;

namespace PetCenterClient.DTOs
{
    public class ReadCategoryDTOs
    {
        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; } = null!;

        public string? CategoryLogo { get; set; }

        public List<ReadCategoryAttributeDTOs>? Attributes { get; set; }
    }

    public class CreateCategoryDTOs
    {
        public string CategoryName { get; set; } = null!;

        public string? CategoryLogo { get; set; }

        public List<CreateCategoryAttributeDTOs>? Attributes { get; set; }
    }

    public class UpdateCategoryDTOs
    {
        public string CategoryName { get; set; } = null!;

        public string? CategoryLogo { get; set; }

        public List<UpdateCategoryAttributeDTOs>? Attributes { get; set; }
    }
}