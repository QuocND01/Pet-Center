using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IFeedbackService
    {
        Task<List<FeedbackDTO>> GetAllAsync();
        Task<FeedbackDTO?> GetDetailAsync(Guid id);
        Task<List<FeedbackDTO>> GetByCustomerAsync(Guid customerId);
        Task<List<FeedbackDTO>> GetByProductAsync(Guid productId);
        Task CreateAsync(CreateFeedbackDTO dto);
        Task ReplyAsync(Guid feedbackId, Guid staffId, string reply);
        Task UpdateReplyAsync(Guid feedbackId, string reply);
        Task DeleteReplyAsync(Guid feedbackId);
        Task ToggleVisibilityAsync(Guid feedbackId);
        Task DeleteAsync(Guid feedbackId);
        Task<List<FeedbackDTO>> FilterAsync(
    int? rating,
    Guid? productId,
    bool? isVisible,
    DateTime? fromDate,
    DateTime? toDate);
    }
}
