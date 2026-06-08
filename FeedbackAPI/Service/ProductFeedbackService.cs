using FeedbackAPI.DTOs;
using FeedbackAPI.Models;
using FeedbackAPI.Repository.Interface;
using FeedbackAPI.Service.Interface;

namespace FeedbackAPI.Service
{
    public class ProductFeedbackService : IProductFeedbackService
    {
        private readonly IProductFeedbackRepository _repository;
        private readonly ICloudinaryService _cloudinaryService;

        private const int MaxMediaPerFeedback = 5;
        private const long MaxImageSizeBytes = 5 * 1024 * 1024;  // 5MB
        private const long MaxVideoSizeBytes = 50 * 1024 * 1024; // 50MB

        public ProductFeedbackService(IProductFeedbackRepository repository,
            ICloudinaryService cloudinaryService)
        {
            _repository = repository;
            _cloudinaryService = cloudinaryService;
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
            foreach (var feedbackDto in dto.Feedbacks)
            {
                if (feedbackDto.MediaFiles == null || !feedbackDto.MediaFiles.Any())
                    continue;

                // Tìm feedback vừa tạo theo ProductId + OrderId
                var savedFeedback = created.FirstOrDefault(f =>
                    f.ProductId == feedbackDto.ProductId &&
                    f.OrderId == feedbackDto.OrderId);

                if (savedFeedback == null) continue;

                var mediaList = await UploadMediaFilesAsync(
                    feedbackDto.MediaFiles, savedFeedback.FeedbackId);

                if (mediaList.Any())
                    await _repository.AddMediaRangeAsync(mediaList);
            }

            // Reload để lấy đủ media trả về
            var result = new List<ProductFeedbackResponseDto>();
            foreach (var f in created)
            {
                var media = await _repository.GetMediaByFeedbackIdAsync(f.FeedbackId);
                f.MediaFiles = media;
                result.Add(MapToResponseDto(f));
            }

            return result;
        }

        public async Task<ProductFeedbackResponseDto?> UpdateFeedbackAsync(
            Guid customerId,
            UpdateProductFeedbackDto dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5.");

            var existing = await _repository.GetFeedbackByIdAsync(dto.FeedbackId);
            if (existing == null) return null;

            if (existing.CustomerId != customerId)
                throw new UnauthorizedAccessException("You are not allowed to edit this feedback.");

            existing.Rating = dto.Rating;
            existing.Comment = dto.Comment;

            var updated = await _repository.UpdateFeedbackAsync(existing);
            if (updated == null) return null;

            // Xóa media cũ nếu có yêu cầu
            if (dto.RemovedPublicIds != null && dto.RemovedPublicIds.Any())
            {
                foreach (var publicId in dto.RemovedPublicIds)
                {
                    // Lấy mediaType để xóa đúng loại trên Cloudinary
                    var allMedia = await _repository.GetMediaByFeedbackIdAsync(dto.FeedbackId);
                    var mediaItem = allMedia.FirstOrDefault(m => m.PublicId == publicId);
                    var mediaType = mediaItem?.MediaType ?? "image";

                    await _cloudinaryService.DeleteMediaAsync(publicId, mediaType);
                }
                await _repository.DeleteMediaByPublicIdsAsync(dto.RemovedPublicIds);
            }

            // Upload media mới nếu có
            if (dto.NewMediaFiles != null && dto.NewMediaFiles.Any())
            {
                var currentMedia = await _repository.GetMediaByFeedbackIdAsync(dto.FeedbackId);
                var currentCount = currentMedia.Count;
                var remainingSlots = MaxMediaPerFeedback - currentCount;

                if (remainingSlots > 0)
                {
                    var filesToUpload = dto.NewMediaFiles.Take(remainingSlots).ToList();
                    var mediaList = await UploadMediaFilesAsync(filesToUpload, dto.FeedbackId);
                    if (mediaList.Any())
                        await _repository.AddMediaRangeAsync(mediaList);
                }
            }

            // Reload media sau khi update
            var updatedMedia = await _repository.GetMediaByFeedbackIdAsync(dto.FeedbackId);
            updated.MediaFiles = updatedMedia;

            return MapToResponseDto(updated);
        }

        // =================== HELPERS ===================

        private async Task<List<FeedbackMedia>> UploadMediaFilesAsync(
            List<IFormFile> files, Guid feedbackId)
        {
            var mediaList = new List<FeedbackMedia>();

            // Giới hạn số lượng
            var limitedFiles = files.Take(MaxMediaPerFeedback).ToList();

            foreach (var file in limitedFiles)
            {
                var contentType = file.ContentType.ToLower();
                var isVideo = contentType.StartsWith("video/");
                var isImage = contentType.StartsWith("image/");

                if (!isImage && !isVideo) continue;

                // Kiểm tra dung lượng
                if (isImage && file.Length > MaxImageSizeBytes) continue;
                if (isVideo && file.Length > MaxVideoSizeBytes) continue;

                string? mediaUrl = null;
                string? publicId = null;
                string mediaType = "image";

                if (isImage)
                {
                    var result = await _cloudinaryService.UploadImageAsync(file, "feedbacks");
                    if (result?.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        mediaUrl = result.SecureUrl.ToString();
                        publicId = result.PublicId;
                        mediaType = "image";
                    }
                }
                else if (isVideo)
                {
                    var result = await _cloudinaryService.UploadVideoAsync(file, "feedbacks");
                    if (result?.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        mediaUrl = result.SecureUrl.ToString();
                        publicId = result.PublicId;
                        mediaType = "video";
                    }
                }

                if (mediaUrl == null) continue;

                mediaList.Add(new FeedbackMedia
                {
                    MediaId = Guid.NewGuid(),
                    FeedbackId = feedbackId,
                    MediaUrl = mediaUrl,
                    PublicId = publicId,
                    MediaType = mediaType,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
            }

            return mediaList;
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
                UpdatedAt = f.UpdatedAt,

                MediaFiles = f.MediaFiles?.Select(m => new FeedbackMediaDto
                {
                    MediaId = m.MediaId,
                    MediaUrl = m.MediaUrl,
                    PublicId = m.PublicId,
                    MediaType = m.MediaType
                }).ToList() ?? new List<FeedbackMediaDto>()
            };
        }
    }
}
