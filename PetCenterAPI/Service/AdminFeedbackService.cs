using PetCenterAPI.DTOs.Requests.ManageFeedback;
using PetCenterAPI.DTOs.Responses.ManageFeedback;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class AdminFeedbackService : IAdminFeedbackService
    {
        private readonly IAdminFeedbackRepository _adminFeedbackRepository;

        public AdminFeedbackService(IAdminFeedbackRepository adminFeedbackRepository)
        {
            _adminFeedbackRepository = adminFeedbackRepository;
        }

        // ============================================================
        // FEEDBACK — VIEW LIST (ADMIN/STAFF)
        // ============================================================
        public async Task<ApiResponse<PagedResult<AdminFeedbackItemResponseDTO>>> GetAllAsync(FeedbackFilterRequestDTO filter)
        {
            var paged = await _adminFeedbackRepository.GetAllAsync(filter);

            int startIndex = (filter.Page - 1) * filter.PageSize + 1;
            for (int i = 0; i < paged.Items.Count; i++)
                paged.Items[i].RowNumber = startIndex + i;

            return ApiResponse<PagedResult<AdminFeedbackItemResponseDTO>>.Ok(paged);
        }
    }
}
