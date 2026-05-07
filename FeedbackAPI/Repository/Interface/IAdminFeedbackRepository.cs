using FeedbackAPI.DTOs;
using FeedbackAPI.Models;

namespace FeedbackAPI.Repository.Interface
{
    public interface IAdminFeedbackRepository
    {
        Task<PagedResult<ProductFeedback>> GetAllAsync(FeedbackFilterDto filter);
        Task<ProductFeedback?> GetByIdAsync(Guid feedbackId);
        Task<bool> ReplyAsync(Guid feedbackId, Guid staffId, string replyContent);
        Task<bool> UpdateReplyAsync(Guid feedbackId, string replyContent);
        Task<bool> DeleteReplyAsync(Guid feedbackId);
        Task<bool> ToggleVisibilityAsync(Guid feedbackId, bool isVisible);
    }
}
