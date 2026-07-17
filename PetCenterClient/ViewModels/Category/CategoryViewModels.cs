using PetCenterClient.Common;
using System.ComponentModel.DataAnnotations;

namespace PetCenterClient.ViewModels.Category
{
    public class ReadCategoryViewModel
    {
        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; } = null!;

        public string? CategoryLogo { get; set; }
        public string? CategoryDescription { get; set; }
        public Status Status { get; set; }
        public List<ReadCategoryAttributeViewModel>? Attributes { get; set; }
    }

    public class ReadCategoryViewModelForCustomer
    {
        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; } = null!;

        public string? CategoryLogo { get; set; }
        public string? CategoryDescription { get; set; }
        public List<ReadCategoryAttributeViewModel>? Attributes { get; set; }
    }

    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(200, ErrorMessage = "Category name cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
       ErrorMessage = "Category name cannot contain special characters")]
        public string CategoryName { get; set; } = null!;

        public IFormFile? CategoryLogo { get; set; }
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? CategoryDescription { get; set; }
        [MaxLength(10, ErrorMessage = "Maximum 10 attributes are allowed.")]
        public List<CreateCategoryAttributeViewModel>? Attributes { get; set; }
    }

    public class UpdateCategoryViewModel
    {
        public Guid CategoryId { get; set; }
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(200, ErrorMessage = "Category name cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
ErrorMessage = "Category name cannot contain special characters")]
        public string CategoryName { get; set; } = null!;

        public IFormFile? CategoryLogo { get; set; }
        public string? ExistingCategoryLogo { get; set; }
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? CategoryDescription { get; set; }
        public Status Status { get; set; }
        [MaxLength(10, ErrorMessage = "Maximum 10 attributes are allowed.")]
        public List<UpdateCategoryAttributeViewModel>? Attributes { get; set; }
    }
}