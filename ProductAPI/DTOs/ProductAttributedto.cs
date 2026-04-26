using System.ComponentModel.DataAnnotations;

namespace ProductAPI.DTOs
{
    public class ProductAttributedto
    {

        public Guid CategoryAttributeId { get; set; }
        public string AttributeName { get; set; } = null!;
        public string? AttributeValue { get; set; }
    }

    public class UpdateProductAttributeDTO
    {
        public Guid CategoryAttributeId { get; set; }

        [Required(ErrorMessage = "Attribute value is required")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
            ErrorMessage = "Attribute value cannot contain special characters")]
        public string AttributeValue { get; set; } = null!;
    }

    public class CreateProductAttributeDTO
    {
        public Guid CategoryAttributeId { get; set; }

        public string? AttributeName { get; set; }

        [Required(ErrorMessage = "Attribute value is required")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
            ErrorMessage = "Attribute value cannot contain special characters")]
        public string? AttributeValue { get; set; }
    }
}
