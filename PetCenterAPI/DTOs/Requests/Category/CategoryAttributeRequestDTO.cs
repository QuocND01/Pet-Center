namespace PetCenterAPI.DTOs.Requests.Category
{
    public class CategoryAttributeRequestDTO
    {
        public class ReadCategoryAttributeDTO
        {

            public Guid CategoryAttributeId { get; set; }
            public Guid CategoryID { get; set; }

            public string? AttributeName { get; set; }
        }
    }
}
