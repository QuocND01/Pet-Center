using PetCenterAPI.DTOs.Requests.ManageFeedback;
using PetCenterAPI.DTOs.Responses.ManageFeedback;

namespace PetCenterAPI.Repository.Interface
{
    public interface IAdminFeedbackRepository
    {
        // ============================================================
        // FEEDBACK — VIEW LIST (ADMIN/STAFF)
        // ============================================================
        Task<PagedResult<AdminFeedbackItemResponseDTO>> GetAllAsync(FeedbackFilterRequestDTO filter);

        // ============================================================
        // FEEDBACK — VIEW DETAIL (ADMIN/STAFF)
        // ============================================================
        Task<AdminFeedbackItemResponseDTO?> GetByIdAsync(Guid feedbackId);

        // ============================================================
        // FEEDBACK — REPLY
        // ============================================================
        Task<bool> ReplyAsync(Guid feedbackId, Guid staffId, string replyContent);

        // ============================================================
        // FEEDBACK — UPDATE REPLY
        // ============================================================
        Task<bool> UpdateReplyAsync(Guid feedbackId, string replyContent);
    }
}
