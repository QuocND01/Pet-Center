using PetCenterAPI.Common;
using static PetCenterAPI.DTOs.Requests.Category.CategoryAttributeRequestDTO;

namespace PetCenterAPI.DTOs.Requests.Category
{
    public class CategoryRequestDTO
    {

        public class ReadCategoryDTO
        {
            public Guid CategoryId { get; set; }

            public string CategoryName { get; set; } = null!;

            public string? CategoryLogo { get; set; }
            public string? CategoryDescription { get; set; }
            public Status Status { get; set; }
            public List<ReadCategoryAttributeDTO>? Attributes { get; set; }
        }

        public class ReadCategoryDTOForCustomer
        {
            public Guid CategoryId { get; set; }

            public string CategoryName { get; set; } = null!;

            public string? CategoryLogo { get; set; }
            public string? CategoryDescription { get; set; }
            public List<ReadCategoryAttributeDTO>? Attributes { get; set; }
        }
    }
}
