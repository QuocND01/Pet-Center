namespace PetCenterClient.DTOs
{
    // ── Request DTOs ──────────────────────────────────────────
    public class ReplyFeedbackDto
    {
        public Guid FeedbackId { get; set; }
        public Guid StaffId { get; set; }
        public string ReplyContent { get; set; } = string.Empty;
    }

    public class UpdateReplyDto
    {
        public Guid FeedbackId { get; set; }
        public string ReplyContent { get; set; } = string.Empty;
    }

    // ── Response DTOs ─────────────────────────────────────────
    public class AdminFeedbackItemDto
    {
        public Guid FeedbackId { get; set; }
        public int RowNumber { get; set; }          // ← thêm

        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }   // ← thêm
        public string? CustomerEmail { get; set; }  // ← thêm

        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }    // ← thêm
        public string? ProductImage { get; set; }   // ← thêm

        public Guid OrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool HasReply { get; set; }
        public string? ReplyContent { get; set; }
        public Guid? StaffId { get; set; }
        public string? StaffName { get; set; }      // ← thêm
        public DateTime? ReplyDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsVisible { get; set; }
    }

    public class FeedbackPagedResult
    {
        public List<AdminFeedbackItemDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class AdminFeedbackApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
