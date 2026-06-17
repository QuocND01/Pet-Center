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
    }
}
