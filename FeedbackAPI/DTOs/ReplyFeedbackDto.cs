namespace FeedbackAPI.DTOs
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

    public class FeedbackFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? Rating { get; set; }
        public bool? HasReply { get; set; }
        public string? Keyword { get; set; }
        public string? SortBy { get; set; }
    }

    public class AdminFeedbackItemDto
    {
        public Guid FeedbackId { get; set; }
        public int RowNumber { get; set; }          // ← STT #1, #2...

        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }   // ← thêm
        public string? CustomerEmail { get; set; }  // ← thêm (chỉ dùng trong detail)

        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }    // ← thêm
        public string? ProductImage { get; set; }   // ← thêm

        public Guid OrderId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public bool HasReply { get; set; }
        public string? ReplyContent { get; set; }
        public Guid? StaffId { get; set; }
        public string? StaffName { get; set; }      // ← thêm
        public DateTime? ReplyDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsVisible { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string? msg = null)
            => new() { Success = true, Data = data, Message = msg };

        public static ApiResponse<T> Fail(string msg)
            => new() { Success = false, Message = msg };
    }
}
