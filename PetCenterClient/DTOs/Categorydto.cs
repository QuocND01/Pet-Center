using PetCenterClient.Common;
using System.ComponentModel.DataAnnotations;

namespace PetCenterClient.DTOs
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

    public class CreateCategoryDTO
    {
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(200, ErrorMessage = "Category name cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
    ErrorMessage = "Category name cannot contain special characters")]
        public string CategoryName { get; set; } = null!;

        public IFormFile? CategoryLogo { get; set; }
        public string? CategoryDescription { get; set; }
        public List<CreateCategoryAttributeDTOs>? Attributes { get; set; }
    }

    public class UpdateCategoryDTO
    {
        public Guid CategoryId { get; set; }
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(200, ErrorMessage = "Category name cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
     ErrorMessage = "Category name cannot contain special characters")]
        public string CategoryName { get; set; } = null!;

        public IFormFile? CategoryLogo { get; set; }
        public string? ExistingCategoryLogo { get; set; }
        public string? CategoryDescription { get; set; }
        public Status Status { get; set; }
        public List<UpdateCategoryAttributeDTOs>? Attributes { get; set; }
    }
}