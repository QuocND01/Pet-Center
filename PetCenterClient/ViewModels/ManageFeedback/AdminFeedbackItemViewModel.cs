namespace PetCenterClient.ViewModels.ManageFeedback
{
    public class AdminFeedbackItemViewModel
    {
        public Guid FeedbackId { get; set; }
        public int RowNumber { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public Guid OrderId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public bool HasReply { get; set; }
        public string? ReplyContent { get; set; }
        public Guid? StaffId { get; set; }
        public string? StaffName { get; set; }
        public DateTime? ReplyDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsVisible { get; set; }

        public List<FeedbackMediaItemViewModel> MediaFiles { get; set; } = new();
    }

    public class FeedbackMediaItemViewModel
    {
        public Guid MediaId { get; set; }
        public string MediaUrl { get; set; } = null!;
        public string? PublicId { get; set; }
        public string MediaType { get; set; } = "image";
    }

    public class FeedbackPagedResultViewModel
    {
        public List<AdminFeedbackItemViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class AdminFeedbackApiResponseViewModel<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }

    public class ReplyFeedbackViewModel
    {
        public Guid FeedbackId { get; set; }
        public Guid StaffId { get; set; }
        public string ReplyContent { get; set; } = string.Empty;
    }

    public class UpdateReplyViewModel
    {
        public Guid FeedbackId { get; set; }
        public string ReplyContent { get; set; } = string.Empty;
    }

    public class ToggleVisibilityViewModel
    {
        public Guid FeedbackId { get; set; }
        public bool IsVisible { get; set; }
    }

    public class FeedbackMediaViewModel
    {
        public Guid MediaId { get; set; }
        public string MediaUrl { get; set; } = null!;
        public string? PublicId { get; set; }
        public string MediaType { get; set; } = "image";
    }

    public class ProductFeedbackViewModel
    {
        public Guid FeedbackId { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public string? Reply { get; set; }
        public DateTime? ReplyDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<FeedbackMediaViewModel> MediaFiles { get; set; } = new();
    }

    public class FeedbackApiResponseViewModel<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }

    public class CheckFeedbackResponseViewModel
    {
        public bool Success { get; set; }
        public bool HasFeedback { get; set; }
    }
}
