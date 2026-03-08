namespace FeedbackAPI.DTOs
{
    public class FeedbackResponseDTO
    {
        public Guid FeedbackID { get; set; }

        public Guid CustomerID { get; set; }

        public Guid ProductID { get; set; }

        public Guid OrderID { get; set; }

        public Guid? StaffID { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public string? Reply { get; set; }

        public DateTime? ReplyDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsVisible { get; set; }
    }
}
