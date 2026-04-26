using System.ComponentModel.DataAnnotations;

namespace ProductAPI.DTOs
{
    public class CreateCategoryAttributeDTOs
    {
        [Required(ErrorMessage = "Category Attribute name is required")]
        [MaxLength(200, ErrorMessage = "Category Attribute name cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
    ErrorMessage = "Category Attribute name cannot contain special characters")]
        public string? AttributeName { get; set; }
    }

    public class ReadCategoryAttributeDTOs
    {

        public Guid CategoryAttributeId { get; set; }
        public Guid CategoryID { get; set; }

        public string? AttributeName { get; set; }
    }


    public class UpdateCategoryAttributeDTOs
    {

        [Required(ErrorMessage = "Category Attribute name is required")]
        [MaxLength(200, ErrorMessage = "Category Attribute name cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
    ErrorMessage = "Category Attribute name cannot contain special characters")]
        public string? AttributeName { get; set; }
    }
}
