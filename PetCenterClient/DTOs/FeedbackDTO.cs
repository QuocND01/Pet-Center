namespace PetCenterClient.DTOs
{
    public class FeedbackDTO
    {
        public Guid FeedbackId { get; set; }

        public Guid CustomerId { get; set; }

        public Guid ProductId { get; set; }

        public Guid OrderId { get; set; }

        public Guid? StaffId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public string? Reply { get; set; }

        public DateTime? ReplyDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsVisible { get; set; }
    }
}
