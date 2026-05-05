namespace PetCenterClient.DTOs;

public class ProductFeedbackResponseDto
{
    public Guid FeedbackId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public string? Reply { get; set; }
    public DateTime? ReplyDate { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateProductFeedbackItemDto
{
    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class CreateBulkFeedbackDto
{
    public List<CreateProductFeedbackItemDto> Feedbacks { get; set; } = new();
}

public class UpdateProductFeedbackDto
{
    public Guid FeedbackId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}