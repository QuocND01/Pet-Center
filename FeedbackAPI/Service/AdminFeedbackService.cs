using AutoMapper;
using FeedbackAPI.DTOs;
using FeedbackAPI.Models;
using FeedbackAPI.Repository.Interface;
using FeedbackAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackAPI.Service
{
    public class AdminFeedbackService : IAdminFeedbackService
    {
        private readonly IAdminFeedbackRepository _repo;
        private readonly IMapper _mapper;
        private readonly IEnrichmentService _enrichment;

        public AdminFeedbackService(IAdminFeedbackRepository repo, IMapper mapper,
        IEnrichmentService enrichment)
        {
            _repo = repo;
            _mapper = mapper;
            _enrichment = enrichment;
        }

        public async Task<ApiResponse<PagedResult<AdminFeedbackItemDto>>> GetAllAsync(FeedbackFilterDto filter)
        {
            var paged = await _repo.GetAllAsync(filter);
            var items = _mapper.Map<List<AdminFeedbackItemDto>>(paged.Items);

            // Gán STT theo page
            int startIndex = (filter.Page - 1) * filter.PageSize + 1;
            for (int i = 0; i < items.Count; i++)
                items[i].RowNumber = startIndex + i;

            // Enrich song song — nếu 1 service lỗi vẫn không ảnh hưởng item khác
            await Task.WhenAll(items.Select(async item =>
            {
                var custTask = _enrichment.GetCustomerInfoAsync(item.CustomerId);
                var productTask = _enrichment.GetProductInfoAsync(item.ProductId);
                await Task.WhenAll(custTask, productTask);

                item.CustomerName = custTask.Result.FullName;
                // CustomerEmail không cần trong list, chỉ dùng trong detail
                item.ProductName = productTask.Result.ProductName;
                item.ProductImage = productTask.Result.ImageUrl;
            }));

            return ApiResponse<PagedResult<AdminFeedbackItemDto>>.Ok(new PagedResult<AdminFeedbackItemDto>
            {
                Items = items,
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize
            });
        }

        public async Task<ApiResponse<AdminFeedbackItemDto>> GetByIdAsync(Guid feedbackId)
        {
            var feedback = await _repo.GetByIdAsync(feedbackId);
            if (feedback == null)
                return ApiResponse<AdminFeedbackItemDto>.Fail("Feedback không tồn tại.");

            var dto = _mapper.Map<AdminFeedbackItemDto>(feedback);

            // Enrich đầy đủ cho detail — gọi song song
            var custTask = _enrichment.GetCustomerInfoAsync(dto.CustomerId);
            var productTask = _enrichment.GetProductInfoAsync(dto.ProductId);
            var staffTask = dto.StaffId.HasValue
                ? _enrichment.GetStaffNameAsync(dto.StaffId.Value)
                : Task.FromResult<string?>(null);

            await Task.WhenAll(custTask, productTask, staffTask);

            (dto.CustomerName, dto.CustomerEmail) = custTask.Result;
            (dto.ProductName, dto.ProductImage) = productTask.Result;
            dto.StaffName = staffTask.Result;

            return ApiResponse<AdminFeedbackItemDto>.Ok(dto);
        }

        public async Task<ApiResponse<bool>> ReplyAsync(ReplyFeedbackDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ReplyContent))
                return ApiResponse<bool>.Fail("Nội dung reply không được để trống.");

            var existing = await _repo.GetByIdAsync(dto.FeedbackId);
            if (existing == null)
                return ApiResponse<bool>.Fail("Feedback không tồn tại.");

            if (!string.IsNullOrEmpty(existing.Reply))
                return ApiResponse<bool>.Fail("Feedback này đã được reply. Hãy dùng chức năng cập nhật.");

            var success = await _repo.ReplyAsync(dto.FeedbackId, dto.StaffId, dto.ReplyContent);
            return success
                ? ApiResponse<bool>.Ok(true, "Reply thành công.")
                : ApiResponse<bool>.Fail("Có lỗi xảy ra khi reply.");
        }

        public async Task<ApiResponse<bool>> UpdateReplyAsync(UpdateReplyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ReplyContent))
                return ApiResponse<bool>.Fail("Nội dung reply không được để trống.");

            var success = await _repo.UpdateReplyAsync(dto.FeedbackId, dto.ReplyContent);
            return success
                ? ApiResponse<bool>.Ok(true, "Cập nhật reply thành công.")
                : ApiResponse<bool>.Fail("Feedback không tồn tại hoặc chưa có reply.");
        }

        public async Task<ApiResponse<bool>> DeleteReplyAsync(Guid feedbackId)
        {
            var success = await _repo.DeleteReplyAsync(feedbackId);
            return success
                ? ApiResponse<bool>.Ok(true, "Đã xóa reply.")
                : ApiResponse<bool>.Fail("Feedback không tồn tại.");
        }

        public async Task<ApiResponse<bool>> ToggleVisibilityAsync(Guid feedbackId, bool isVisible)
        {
            var success = await _repo.ToggleVisibilityAsync(feedbackId, isVisible);
            return success
                ? ApiResponse<bool>.Ok(true, isVisible ? "Đã hiện feedback." : "Đã ẩn feedback.")
                : ApiResponse<bool>.Fail("Feedback không tồn tại.");
        }
    }
}
