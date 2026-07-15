using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Responses.Product
{
    public class ProductAttributeResponseDTO
    {

        public class UpdateProductAttributeDTO
        {
            public Guid CategoryAttributeId { get; set; }

            [Required(ErrorMessage = "Attribute value is required")]
            [MaxLength(200, ErrorMessage = "Attribute value cannot exceed 200 characters")]
            [RegularExpression(@"^[a-zA-Z0-9\s]+$",
                ErrorMessage = "Attribute value cannot contain special characters")]
            public string AttributeValue { get; set; } = null!;
        }

        public class CreateProductAttributeDTO
        {
            public Guid CategoryAttributeId { get; set; }

            public string? AttributeName { get; set; }

            [Required(ErrorMessage = "Attribute value is required")]
            [MaxLength(200, ErrorMessage = "Attribute value cannot exceed 200 characters")]
            [RegularExpression(@"^[a-zA-Z0-9\s]+$",
                ErrorMessage = "Attribute value cannot contain special characters")]
            public string? AttributeValue { get; set; }
        }


        public class ProductAttributeCompareDTO
        {
            public Guid CategoryAttributeId { get; set; }
            public string AttributeValue { get; set; } = null!;
        }

    }
}
