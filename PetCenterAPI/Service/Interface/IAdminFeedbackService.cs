using PetCenterAPI.DTOs.Requests.ManageFeedback;
using PetCenterAPI.DTOs.Responses.ManageFeedback;

namespace PetCenterAPI.Service.Interface
{
    public interface IAdminFeedbackService
    {
        // ============================================================
        // FEEDBACK — VIEW LIST (ADMIN/STAFF)
        // ============================================================
        Task<ApiResponse<PagedResult<AdminFeedbackItemResponseDTO>>> GetAllAsync(FeedbackFilterRequestDTO filter);

        // ============================================================
        // FEEDBACK — VIEW DETAIL (ADMIN/STAFF)
        // ============================================================
        Task<ApiResponse<AdminFeedbackItemResponseDTO>> GetByIdAsync(Guid feedbackId);

        // ============================================================
        // FEEDBACK — REPLY
        // ============================================================
        Task<ApiResponse<bool>> ReplyAsync(ReplyFeedbackRequestDTO request);

        // ============================================================
        // FEEDBACK — UPDATE REPLY
        // ============================================================
        Task<ApiResponse<bool>> UpdateReplyAsync(UpdateReplyRequestDTO request);

        // ============================================================
        // FEEDBACK — DELETE REPLY
        // ============================================================
        Task<ApiResponse<bool>> DeleteReplyAsync(Guid feedbackId);

        // ============================================================
        // FEEDBACK — TOGGLE VISIBILITY
        // ============================================================
        Task<ApiResponse<bool>> ToggleVisibilityAsync(Guid feedbackId, bool isVisible);
    }
}
