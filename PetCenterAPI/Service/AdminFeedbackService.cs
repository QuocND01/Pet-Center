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

        // ============================================================
        // FEEDBACK — VIEW DETAIL (ADMIN/STAFF)
        // ============================================================
        public async Task<ApiResponse<AdminFeedbackItemResponseDTO>> GetByIdAsync(Guid feedbackId)
        {
            var feedback = await _adminFeedbackRepository.GetByIdAsync(feedbackId);
            if (feedback == null)
                return ApiResponse<AdminFeedbackItemResponseDTO>.Fail("Feedback Not Found.");

            return ApiResponse<AdminFeedbackItemResponseDTO>.Ok(feedback);
        }

        // ============================================================
        // FEEDBACK — REPLY
        // ============================================================
        public async Task<ApiResponse<bool>> ReplyAsync(ReplyFeedbackRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.ReplyContent))
                return ApiResponse<bool>.Fail("Reply content cannot be empty.");

            var existing = await _adminFeedbackRepository.GetByIdAsync(request.FeedbackId);
            if (existing == null)
                return ApiResponse<bool>.Fail("Feedback does not exist.");

            if (!string.IsNullOrEmpty(existing.ReplyContent))
                return ApiResponse<bool>.Fail("This feedback already has a reply. Please use the update function.");

            var success = await _adminFeedbackRepository.ReplyAsync(
                request.FeedbackId, request.StaffId, request.ReplyContent);

            return success
                ? ApiResponse<bool>.Ok(true, "Reply submitted successfully.")
                : ApiResponse<bool>.Fail("An error occurred while submitting the reply.");
        }

        // ============================================================
        // FEEDBACK — UPDATE REPLY
        // ============================================================
        public async Task<ApiResponse<bool>> UpdateReplyAsync(UpdateReplyRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.ReplyContent))
                return ApiResponse<bool>.Fail("Reply content cannot be empty.");

            var success = await _adminFeedbackRepository.UpdateReplyAsync(
                request.FeedbackId, request.ReplyContent);

            return success
                ? ApiResponse<bool>.Ok(true, "Reply updated successfully.")
                : ApiResponse<bool>.Fail("Feedback does not exist or has no reply yet.");
        }

        // ============================================================
        // FEEDBACK — DELETE REPLY
        // ============================================================
        public async Task<ApiResponse<bool>> DeleteReplyAsync(Guid feedbackId)
        {
            var success = await _adminFeedbackRepository.DeleteReplyAsync(feedbackId);

            return success
                ? ApiResponse<bool>.Ok(true, "Reply deleted.")
                : ApiResponse<bool>.Fail("Feedback does not exist.");
        }

        // ============================================================
        // FEEDBACK — TOGGLE VISIBILITY
        // ============================================================
        public async Task<ApiResponse<bool>> ToggleVisibilityAsync(Guid feedbackId, bool isVisible)
        {
            var success = await _adminFeedbackRepository.ToggleVisibilityAsync(feedbackId, isVisible);

            return success
                ? ApiResponse<bool>.Ok(true, isVisible ? "Feedback is now visible." : "Feedback has been hidden.")
                : ApiResponse<bool>.Fail("Feedback does not exist.");
        }
    }
}
