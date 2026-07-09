namespace PetCenterClient.ViewModels
{
    public class ChatCustomerViewModel
    {
        public Guid CustomerId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string LastMessage { get; set; } = "";
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ChatMessageViewModel
    {
        public Guid MessageId { get; set; }
        public Guid SenderId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}