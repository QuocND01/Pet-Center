using PetCenterAPI.Common;
using System.ComponentModel.DataAnnotations;
using static PetCenterAPI.DTOs.Responses.Category.CategoryAttributeResponseDTO;

namespace PetCenterAPI.DTOs.Responses.Category
{
    public class CategoryResponseDTO
    {

        public class CreateCategoryDTO
        {
            public Guid CategoryId { get; set; }
            [Required(ErrorMessage = "Category name is required")]
            [MaxLength(200, ErrorMessage = "Category name cannot exceed 200 characters")]
            [RegularExpression(@"^[a-zA-Z0-9\s]+$",
        ErrorMessage = "Category name cannot contain special characters")]
            public string CategoryName { get; set; } = null!;

            public IFormFile? CategoryLogo { get; set; }
            public string? CategoryDescription { get; set; }
            public Status Status { get; set; }
            public List<CreateCategoryAttributeDTO>? Attributes { get; set; }
        }

        public class UpdateCategoryDTO
        {
            [Required(ErrorMessage = "Category name is required")]
            [MaxLength(200, ErrorMessage = "Category name cannot exceed 200 characters")]
            [RegularExpression(@"^[a-zA-Z0-9\s]+$",
        ErrorMessage = "Category name cannot contain special characters")]
            public string CategoryName { get; set; } = null!;

            public IFormFile? CategoryLogo { get; set; }
            public string? CategoryDescription { get; set; }

            public List<UpdateCategoryAttributeDTO>? Attributes { get; set; }
        }
    }
}
