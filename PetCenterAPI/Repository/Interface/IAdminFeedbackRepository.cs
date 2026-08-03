using PetCenterAPI.DTOs.Requests.ManageFeedback;
using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IAdminFeedbackRepository
    {
        // ============================================================
        // FEEDBACK — VIEW LIST (ADMIN/STAFF)
        // ============================================================
        Task<(List<ProductFeedback> Items, int TotalCount)> GetAllAsync(FeedbackFilterRequestDTO filter);

        // ============================================================
        // FEEDBACK — VIEW DETAIL (ADMIN/STAFF)
        // ============================================================
        Task<ProductFeedback?> GetByIdAsync(Guid feedbackId);

        // ============================================================
        // FEEDBACK — REPLY
        // ============================================================
        Task<bool> ReplyAsync(Guid feedbackId, Guid staffId, string replyContent);

        // ============================================================
        // FEEDBACK — UPDATE REPLY
        // ============================================================
        Task<bool> UpdateReplyAsync(Guid feedbackId, string replyContent);

        // ============================================================
        // FEEDBACK — DELETE REPLY
        // ============================================================
        Task<bool> DeleteReplyAsync(Guid feedbackId);

        // ============================================================
        // FEEDBACK — TOGGLE VISIBILITY
        // ============================================================
        Task<bool> ToggleVisibilityAsync(Guid feedbackId, bool isVisible);
    }
}
