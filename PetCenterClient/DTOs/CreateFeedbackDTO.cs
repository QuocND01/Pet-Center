namespace PetCenterClient.DTOs
{
    public class CreateFeedbackDTO
    {
        public Guid ProductId { get; set; }

        public Guid OrderId { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; }
    }
}
