namespace PetCenterClient.DTOs
{
    public class ProductAttributedto
    {
        public string AttributeName { get; set; } = null!;
        public string? AttributeValue { get; set; }
    }

    public class UpdateProductAttributeDTO
    {
        public Guid CategoryAttributeId { get; set; }
        public string AttributeValue { get; set; } = null!;
    }


    public class CreateProductAttributeDTO
    {
        public Guid CategoryAttributeId { get; set; }
        public string AttributeValue { get; set; } = null!;
    }
}
