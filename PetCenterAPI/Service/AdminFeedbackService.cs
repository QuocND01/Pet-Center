using PetCenterAPI.DTOs.Requests.ManageFeedback;
using PetCenterAPI.DTOs.Responses.ManageFeedback;
using PetCenterAPI.Models;
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
            var (items, totalCount) = await _adminFeedbackRepository.GetAllAsync(filter);

            int startIndex = (filter.Page - 1) * filter.PageSize + 1;
            var dtoList = items.Select((f, index) => new AdminFeedbackItemResponseDTO
            {
                RowNumber = startIndex + index,
                FeedbackId = f.FeedbackId,
                CustomerId = f.CustomerId,
                CustomerName = f.Customer?.FullName,
                CustomerEmail = f.Customer?.Email,
                ProductId = f.ProductId,
                ProductName = f.Product?.ProductName,
                ProductImage = f.Product?.ProductImages?
                    .Where(pi => pi.IsActive == true)
                    .Select(pi => pi.ImageUrl)
                    .FirstOrDefault(),
                OrderId = f.OrderId,
                Rating = f.Rating,
                Comment = f.Comment,
                ReplyContent = f.Reply,
                HasReply = !string.IsNullOrEmpty(f.Reply),
                StaffId = f.StaffId,
                StaffName = f.Staff?.FullName,
                ReplyDate = f.ReplyDate,
                CreatedDate = f.CreatedAt,
                IsVisible = f.Status == 1
            }).ToList();

            var pagedResult = new PagedResult<AdminFeedbackItemResponseDTO>
            {
                Items = dtoList,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };

            return ApiResponse<PagedResult<AdminFeedbackItemResponseDTO>>.Ok(pagedResult);
        }

        // ============================================================
        // FEEDBACK — VIEW DETAIL (ADMIN/STAFF)
        // ============================================================
        public async Task<ApiResponse<AdminFeedbackItemResponseDTO>> GetByIdAsync(Guid feedbackId)
        {
            var f = await _adminFeedbackRepository.GetByIdAsync(feedbackId);
            if (f == null)
                return ApiResponse<AdminFeedbackItemResponseDTO>.Fail("Feedback Not Found.");

            var dto = new AdminFeedbackItemResponseDTO
            {
                FeedbackId = f.FeedbackId,
                CustomerId = f.CustomerId,
                CustomerName = f.Customer?.FullName,
                CustomerEmail = f.Customer?.Email,
                ProductId = f.ProductId,
                ProductName = f.Product?.ProductName,
                ProductImage = f.Product?.ProductImages?
                    .Where(pi => pi.IsActive == true)
                    .Select(pi => pi.ImageUrl)
                    .FirstOrDefault(),
                OrderId = f.OrderId,
                Rating = f.Rating,
                Comment = f.Comment,
                ReplyContent = f.Reply,
                HasReply = !string.IsNullOrEmpty(f.Reply),
                StaffId = f.StaffId,
                StaffName = f.Staff?.FullName,
                ReplyDate = f.ReplyDate,
                CreatedDate = f.CreatedAt,
                IsVisible = f.Status == 1,
                MediaFiles = f.FeedbackImages != null
                    ? f.FeedbackImages
                        .Where(img => img.IsActive == true)
                        .Select(img => new FeedbackMediaItemDTO
                        {
                            MediaId = img.ImageId,
                            MediaUrl = img.ImageUrl,
                            PublicId = img.PublicId,
                            MediaType = DetermineMediaType(img.ImageUrl)
                        })
                        .ToList()
                    : new List<FeedbackMediaItemDTO>()
            };

            return ApiResponse<AdminFeedbackItemResponseDTO>.Ok(dto);
        }

        private static string DetermineMediaType(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return "image";
            if (imageUrl.Contains("/video/upload/", StringComparison.OrdinalIgnoreCase)) return "video";
            if (imageUrl.Contains("/image/upload/", StringComparison.OrdinalIgnoreCase)) return "image";
            if (imageUrl.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                imageUrl.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
                imageUrl.EndsWith(".webm", StringComparison.OrdinalIgnoreCase))
                return "video";
            return "image";
        }

        // ============================================================
        // FEEDBACK — REPLY
        // ============================================================
        public async Task<ApiResponse<bool>> ReplyAsync(ReplyFeedbackRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.ReplyContent))
                return ApiResponse<bool>.Fail("Reply content cannot be empty.");

            if (request.ReplyContent.Length > 1000)
                return ApiResponse<bool>.Fail("Reply content cannot exceed 1000 characters.");

            var existing = await _adminFeedbackRepository.GetByIdAsync(request.FeedbackId);
            if (existing == null)
                return ApiResponse<bool>.Fail("Feedback does not exist.");

            if (!string.IsNullOrEmpty(existing.Reply))
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

            if (request.ReplyContent.Length > 1000)
                return ApiResponse<bool>.Fail("Reply content cannot exceed 1000 characters.");

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
