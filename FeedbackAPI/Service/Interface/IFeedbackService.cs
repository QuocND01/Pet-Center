using FeedbackAPI.DTOs;

namespace FeedbackAPI.Service.Interface
{
    public interface IFeedbackService
    {
        Task CreateFeedbackAsync(CreateFeedbackDTO dto);
        Task<List<FeedbackResponseDTO>> GetByProductAsync(Guid productId);
        Task ReplyFeedbackAsync(Guid feedbackId, string reply);
    }
}
