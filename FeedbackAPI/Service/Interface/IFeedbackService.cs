using FeedbackAPI.DTOs;

namespace FeedbackAPI.Service.Interface
{
    public interface IFeedbackService
    {
        //Customer
        Task CreateFeedbackAsync(CreateFeedbackDTO dto);
        Task<List<FeedbackResponseDTO>> GetByProductAsync(Guid productId);
        Task<List<FeedbackResponseDTO>> GetByCustomerAsync(Guid customerId);

        //Detail
        Task<FeedbackResponseDTO?> GetDetailAsync(Guid feedbackId);

        //Admin
        Task<List<FeedbackResponseDTO>> GetAllForAdminAsync();
        Task<List<FeedbackResponseDTO>> FilterAsync(
            int? rating,
            Guid? productId,
            bool? isVisible,
            DateTime? fromDate,
            DateTime? toDate);

        //Soft delete
        Task DeleteFeedbackAsync(Guid feedbackId);

        //Reply
        Task ReplyFeedbackAsync(Guid feedbackId, Guid staffId, string reply);
        Task UpdateReplyAsync(Guid feedbackId, string reply);
        Task DeleteReplyAsync(Guid feedbackId);

        //Visibility
        Task ToggleVisibilityAsync(Guid feedbackId);
    }
}

