using System.ComponentModel.DataAnnotations;

namespace ProductAPI.DTOs
{
    public class ReadCategoryDTOs
    {
        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; } = null!;

        public string? CategoryLogo { get; set; }
        public string? CategoryDescription { get; set; }

        public List<ReadCategoryAttributeDTOs>? Attributes { get; set; }
    }

    public class CreateCategoryDTOs
    {
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(200, ErrorMessage = "Category name cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
    ErrorMessage = "Category name cannot contain special characters")]
        public string CategoryName { get; set; } = null!;

        public string? CategoryLogo { get; set; }
        public string? CategoryDescription { get; set; }

        public List<CreateCategoryAttributeDTOs>? Attributes { get; set; }
    }

    public class UpdateCategoryDTOs
    {
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(200, ErrorMessage = "Category name cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
    ErrorMessage = "Category name cannot contain special characters")]
        public string CategoryName { get; set; } = null!;

        public string? CategoryLogo { get; set; }
        public string? CategoryDescription { get; set; }

        public List<UpdateCategoryAttributeDTOs>? Attributes { get; set; }
    }
}