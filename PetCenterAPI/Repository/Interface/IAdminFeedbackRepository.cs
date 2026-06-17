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
    }
}
