using FeedbackAPI.DTOs;

namespace FeedbackAPI.Service.Interface
{
    public interface IAdminFeedbackService
    {
        Task<ApiResponse<PagedResult<AdminFeedbackItemDto>>> GetAllAsync(FeedbackFilterDto filter);
        Task<ApiResponse<AdminFeedbackItemDto>> GetByIdAsync(Guid feedbackId);
        Task<ApiResponse<bool>> ReplyAsync(ReplyFeedbackDto dto);
        Task<ApiResponse<bool>> UpdateReplyAsync(UpdateReplyDto dto);
        Task<ApiResponse<bool>> DeleteReplyAsync(Guid feedbackId);
        Task<ApiResponse<bool>> ToggleVisibilityAsync(Guid feedbackId, bool isVisible);
    }
}
