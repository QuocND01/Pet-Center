namespace PetCenterAPI.DTOs.Requests.ManageFeedback
{
    public class FeedbackFilterRequestDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? Rating { get; set; }
        public bool? HasReply { get; set; }
        public string? Keyword { get; set; }
        public string? SortBy { get; set; }
    }
}
