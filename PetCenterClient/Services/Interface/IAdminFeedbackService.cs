using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IAdminFeedbackService
    {
        Task<FeedbackPagedResult?> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            int? rating = null,
            bool? hasReply = null,
            string? keyword = null,
            string? sortBy = null);

        Task<AdminFeedbackItemDto?> GetByIdAsync(Guid feedbackId);
        Task<(bool success, string message)> ReplyAsync(ReplyFeedbackDto dto);
        Task<(bool success, string message)> UpdateReplyAsync(UpdateReplyDto dto);
        Task<(bool success, string message)> DeleteReplyAsync(Guid feedbackId);
        Task<(bool success, string message)> ToggleVisibilityAsync(Guid feedbackId, bool isVisible);
    }
}
