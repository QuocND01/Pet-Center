using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Responses.Category
{
    public class CategoryAttributeResponseDTO
    {
        public class CreateCategoryAttributeDTO
        {
            [Required(ErrorMessage = "Category Attribute name is required")]
            [MaxLength(200, ErrorMessage = "Category Attribute name cannot exceed 200 characters")]
            [RegularExpression(@"^[a-zA-Z0-9\s]+$",
        ErrorMessage = "Category Attribute name cannot contain special characters")]
            public string? AttributeName { get; set; }
        }

        public class UpdateCategoryAttributeDTO
        {
            public Guid? CategoryAttributeId { get; set; }

            [Required(ErrorMessage = "Category Attribute name is required")]
            [MaxLength(200, ErrorMessage = "Category Attribute name cannot exceed 200 characters")]
            [RegularExpression(@"^[a-zA-Z0-9\s]+$",
                ErrorMessage = "Category Attribute name cannot contain special characters")]
            public string? AttributeName { get; set; }
        }
    }
}
