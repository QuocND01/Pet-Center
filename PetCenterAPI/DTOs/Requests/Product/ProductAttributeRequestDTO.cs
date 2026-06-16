namespace PetCenterAPI.DTOs.Requests.Product
{
    public class ProductAttributeRequestDTO
    {
        public class ProductAttributeDTO
        {

            public Guid CategoryAttributeId { get; set; }
            public string AttributeName { get; set; } = null!;
            public string? AttributeValue { get; set; }
        }

    }
}
