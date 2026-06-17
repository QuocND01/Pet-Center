using PetCenterClient.DTOs;
using PetCenterClient.ViewModels.ManageFeedback;

namespace PetCenterClient.Services.Interface
{
    public interface IAdminFeedbackApiService
    {
        // ============================================================
        // FEEDBACK — VIEW LIST (ADMIN/STAFF)
        // ============================================================
        Task<FeedbackPagedResultViewModel?> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            int? rating = null,
            bool? hasReply = null,
            string? keyword = null,
            string? sortBy = null);

        // ============================================================
        // FEEDBACK — VIEW DETAIL (ADMIN/STAFF)
        // ============================================================
        Task<AdminFeedbackItemViewModel?> GetByIdAsync(Guid feedbackId);

        // ============================================================
        // FEEDBACK — REPLY
        // ============================================================
        Task<(bool success, string message)> ReplyAsync(ReplyFeedbackViewModel dto);

        // ============================================================
        // FEEDBACK — UPDATE REPLY
        // ============================================================
        Task<(bool success, string message)> UpdateReplyAsync(UpdateReplyViewModel dto);
        Task<(bool success, string message)> DeleteReplyAsync(Guid feedbackId);
        Task<(bool success, string message)> ToggleVisibilityAsync(Guid feedbackId, bool isVisible);
    }
}
