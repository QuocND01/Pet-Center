using FeedbackAPI.DTOs;
using FeedbackAPI.Models;
using FeedbackAPI.Repository.Interface;
using FeedbackAPI.Service.Interface;

namespace FeedbackAPI.Service
{
    public class ProductFeedbackService : IProductFeedbackService
    {
        private readonly IProductFeedbackRepository _repository;

        public ProductFeedbackService(IProductFeedbackRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ProductFeedbackResponseDto>> GetFeedbacksByProductIdAsync(Guid productId)
        {
            var feedbacks = await _repository.GetFeedbacksByProductIdAsync(productId);
            return feedbacks.Select(MapToResponseDto).ToList();
        }

        public async Task<List<ProductFeedbackResponseDto>> GetFeedbacksByOrderIdAsync(Guid orderId)
        {
            var feedbacks = await _repository.GetFeedbacksByOrderIdAsync(orderId);
            return feedbacks.Select(MapToResponseDto).ToList();
        }

        public async Task<bool> HasFeedbackForOrderAsync(Guid orderId, Guid customerId)
        {
            return await _repository.HasFeedbackForOrderAsync(orderId, customerId);
        }

        public async Task<List<ProductFeedbackResponseDto>> CreateBulkFeedbackAsync(
            Guid customerId,
            CreateBulkFeedbackDto dto)
        {
            // Validate không được gửi list rỗng
            if (dto.Feedbacks == null || dto.Feedbacks.Count == 0)
                throw new ArgumentException("Feedback list cannot be empty.");

            // Validate rating 1-5
            if (dto.Feedbacks.Any(f => f.Rating < 1 || f.Rating > 5))
                throw new ArgumentException("Rating must be between 1 and 5.");

            // Kiểm tra order này customer đã feedback chưa
            var orderId = dto.Feedbacks.First().OrderId;
            var hasFeedback = await _repository.HasFeedbackForOrderAsync(orderId, customerId);
            if (hasFeedback)
                throw new InvalidOperationException("This order has already been reviewed.");

            // Map DTO -> Model
            var feedbacks = dto.Feedbacks.Select(f => new ProductFeedback
            {
                FeedbackId = Guid.NewGuid(),
                CustomerId = customerId,
                ProductId = f.ProductId,
                OrderId = f.OrderId,
                Rating = f.Rating,
                Comment = f.Comment,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsVisible = true
            }).ToList();

            var created = await _repository.CreateBulkFeedbackAsync(feedbacks);
            return created.Select(MapToResponseDto).ToList();
        }

        public async Task<ProductFeedbackResponseDto?> UpdateFeedbackAsync(
            Guid customerId,
            UpdateProductFeedbackDto dto)
        {
            // Validate rating 1-5
            if (dto.Rating < 1 || dto.Rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5.");

            // Lấy feedback hiện tại
            var existing = await _repository.GetFeedbackByIdAsync(dto.FeedbackId);
            if (existing == null)
                return null;

            // Đảm bảo customer chỉ sửa feedback của chính mình
            if (existing.CustomerId != customerId)
                throw new UnauthorizedAccessException("You are not allowed to edit this feedback.");

            existing.Rating = dto.Rating;
            existing.Comment = dto.Comment;

            var updated = await _repository.UpdateFeedbackAsync(existing);
            return updated == null ? null : MapToResponseDto(updated);
        }

        // Helper map Model -> ResponseDto
        private static ProductFeedbackResponseDto MapToResponseDto(ProductFeedback f)
        {
            return new ProductFeedbackResponseDto
            {
                FeedbackId = f.FeedbackId,
                CustomerId = f.CustomerId,
                ProductId = f.ProductId,
                OrderId = f.OrderId,
                Rating = f.Rating,
                Comment = f.Comment,
                Reply = f.Reply,
                ReplyDate = f.ReplyDate,
                CreatedDate = f.CreatedDate,
                UpdatedAt = f.UpdatedAt
            };
        }
    }
}
