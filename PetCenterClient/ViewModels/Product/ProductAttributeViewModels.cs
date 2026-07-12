using System.ComponentModel.DataAnnotations;

namespace PetCenterClient.ViewModels.Product
{
    public class ProductAttributeViewModels
    {
        public Guid CategoryAttributeId { get; set; }
        public string AttributeName { get; set; } = null!;
        public string? AttributeValue { get; set; }
    }

    public class UpdateProductAttributeViewModel
    {
        public Guid CategoryAttributeId { get; set; }
        public string AttributeName { get; set; } = null!;

        [Required(ErrorMessage = "Attribute value is required")]
        [MaxLength(200, ErrorMessage = "Attribute value cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
                ErrorMessage = "Attribute value cannot contain special characters")]
        public string AttributeValue { get; set; } = null!;
    }

    public class CreateProductAttributeViewModel
    {
        public Guid CategoryAttributeId { get; set; }

        public string? AttributeName { get; set; } = null!;

        [Required(ErrorMessage = "Attribute value is required")]
        [MaxLength(200, ErrorMessage = "Attribute value cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
                 ErrorMessage = "Attribute value cannot contain special characters")]
        public string AttributeValue { get; set; } = null!;
    }
}
