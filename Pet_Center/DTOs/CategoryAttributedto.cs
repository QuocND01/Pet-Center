namespace ProductAPI.DTOs
{
    public class CreateCategoryAttributeDTOs
    {

        public Guid CategoryAttribute { get; set; }
        public Guid CategoryID { get; set; }

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
        public Guid CategoryID { get; set; }

        public string? AttributeName { get; set; }
    }
}
