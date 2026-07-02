using PetCenterAPI.DTOs.Requests.ManageFeedback;
using PetCenterAPI.DTOs.Responses.ManageFeedback;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class ProductFeedbackService : IProductFeedbackService
    {
        private readonly IProductFeedbackRepository _productFeedbackRepository;
        private readonly ICloudinaryService _cloudinaryService;

        private const int MaxImagesPerFeedback = 2;
        private const int MaxVideosPerFeedback = 1;
        private const int MaxMediaPerFeedback = MaxImagesPerFeedback + MaxVideosPerFeedback;
        private const long MaxImageSizeBytes = 5 * 1024 * 1024;   
        private const long MaxVideoSizeBytes = 30 * 1024 * 1024;

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".mov", ".webm" };
        private static readonly string[] VideoExtensions = AllowedVideoExtensions;

        public ProductFeedbackService(IProductFeedbackRepository productFeedbackRepository, ICloudinaryService cloudinaryService)
        {
            _productFeedbackRepository = productFeedbackRepository;
            _cloudinaryService = cloudinaryService;
        }

        // ============================================================
        // FEEDBACK — VIEW BY PRODUCT (CUSTOMER SIDE)
        // ============================================================
        public async Task<List<ProductFeedbackResponseDTO>> GetFeedbacksByProductIdAsync(Guid productId)
        {
            var feedbacks = await _productFeedbackRepository.GetFeedbacksByProductIdAsync(productId);
            return feedbacks.Select(MapToResponse).ToList();
        }

        // ============================================================
        // FEEDBACK — VIEW BY ORDER (CUSTOMER SIDE — ORDER DETAIL POPUP)
        // ============================================================
        public async Task<List<ProductFeedbackResponseDTO>> GetFeedbacksByOrderIdAsync(Guid orderId)
        {
            var feedbacks = await _productFeedbackRepository.GetFeedbacksByOrderIdAsync(orderId);
            return feedbacks.Select(MapToResponse).ToList();
        }

        public async Task<bool> HasFeedbackForOrderAsync(Guid orderId, Guid customerId)
        {
            return await _productFeedbackRepository.HasFeedbackForOrderAsync(orderId, customerId);
        }

        // ============================================================
        // FEEDBACK — CREATE BULK (CUSTOMER SIDE)
        // ============================================================
        public async Task<List<ProductFeedbackResponseDTO>> CreateBulkFeedbackAsync(
            Guid customerId,
            CreateBulkFeedbackRequestDTO request)
        {
            if (request.Feedbacks == null || request.Feedbacks.Count == 0)
                throw new ArgumentException("Feedback list cannot be empty.");

            if (request.Feedbacks.Any(f => f.Rating < 1 || f.Rating > 5))
                throw new ArgumentException("Rating must be between 1 and 5.");

            var orderId = request.Feedbacks.First().OrderId;
            var alreadyReviewed = await _productFeedbackRepository.HasFeedbackForOrderAsync(orderId, customerId);
            if (alreadyReviewed)
                throw new InvalidOperationException("This order has already been reviewed.");

            var feedbacks = request.Feedbacks.Select(f => new ProductFeedback
            {
                FeedbackId = Guid.NewGuid(),
                CustomerId = customerId,
                ProductId = f.ProductId,
                OrderId = f.OrderId,
                Rating = f.Rating,
                Comment = f.Comment,
                CreatedAt = DateTime.Now,
                Status = 1
            }).ToList();

            var created = await _productFeedbackRepository.CreateBulkAsync(feedbacks);

            foreach (var itemRequest in request.Feedbacks)
            {
                if (itemRequest.MediaFiles == null || !itemRequest.MediaFiles.Any())
                    continue;

                var savedFeedback = created.FirstOrDefault(f =>
                    f.ProductId == itemRequest.ProductId &&
                    f.OrderId == itemRequest.OrderId);
                if (savedFeedback == null) continue;

                var mediaList = await UploadMediaFilesAsync(itemRequest.MediaFiles, savedFeedback.FeedbackId);
                if (mediaList.Any())
                    await _productFeedbackRepository.AddMediaRangeAsync(mediaList);
            }

            var result = new List<ProductFeedbackResponseDTO>();
            foreach (var f in created)
            {
                var images = await _productFeedbackRepository.GetImagesByFeedbackIdAsync(f.FeedbackId);
                f.FeedbackImages = images;
                result.Add(MapToResponse(f));
            }
            return result;
        }

        // ============================================================
        // FEEDBACK — UPDATE (CUSTOMER SIDE)
        // ============================================================
        public async Task<ProductFeedbackResponseDTO?> UpdateFeedbackAsync(
            Guid customerId,
            UpdateFeedbackRequestDTO request)
        {
            if (request.Rating < 1 || request.Rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5.");

            var existing = await _productFeedbackRepository.GetFeedbackByIdAsync(request.FeedbackId);
            if (existing == null) return null;

            if (existing.CustomerId != customerId)
                throw new UnauthorizedAccessException("You are not allowed to edit this feedback.");

            existing.Rating = request.Rating;
            existing.Comment = request.Comment;

            var updated = await _productFeedbackRepository.UpdateFeedbackAsync(existing);
            if (updated == null) return null;

            if (request.RemovedPublicIds != null && request.RemovedPublicIds.Any())
            {
                var allMedia = await _productFeedbackRepository.GetImagesByFeedbackIdAsync(request.FeedbackId);

                foreach (var publicId in request.RemovedPublicIds)
                {
                    var mediaItem = allMedia.FirstOrDefault(m => m.PublicId == publicId);
                    var mediaType = DetectMediaType(mediaItem?.ImageUrl ?? "");
                    await _cloudinaryService.DeleteMediaAsync(publicId, mediaType);
                }

                await _productFeedbackRepository.DeleteMediaByPublicIdsAsync(request.RemovedPublicIds);
            }

            if (request.NewMediaFiles != null && request.NewMediaFiles.Any())
            {
                var currentMedia = await _productFeedbackRepository.GetImagesByFeedbackIdAsync(request.FeedbackId);

                var currentImages = currentMedia.Count(m => DetectMediaType(m.ImageUrl) == "image");
                var currentVideos = currentMedia.Count(m => DetectMediaType(m.ImageUrl) == "video");

                var remainingImageSlots = MaxImagesPerFeedback - currentImages;
                var remainingVideoSlots = MaxVideosPerFeedback - currentVideos;

                if (remainingImageSlots > 0 || remainingVideoSlots > 0)
                {
                    var newImages = request.NewMediaFiles
                        .Where(f => f.ContentType.StartsWith("image/"))
                        .Take(remainingImageSlots)
                        .ToList();
                    var newVideos = request.NewMediaFiles
                        .Where(f => f.ContentType.StartsWith("video/"))
                        .Take(remainingVideoSlots)
                        .ToList();

                    var toUpload = newImages.Concat(newVideos).ToList();
                    if (toUpload.Any())
                    {
                        var mediaList = await UploadMediaFilesAsync(toUpload, request.FeedbackId);
                        if (mediaList.Any())
                            await _productFeedbackRepository.AddMediaRangeAsync(mediaList);
                    }
                }
            }

            var finalImages = await _productFeedbackRepository.GetImagesByFeedbackIdAsync(request.FeedbackId);
            updated.FeedbackImages = finalImages;
            return MapToResponse(updated);
        }

        // ============================================================
        // HELPER
        // ============================================================
        private static ProductFeedbackResponseDTO MapToResponse(ProductFeedback feedback)
        {
            return new ProductFeedbackResponseDTO
            {
                FeedbackId = feedback.FeedbackId,
                CustomerId = feedback.CustomerId,
                CustomerName = feedback.Customer?.FullName,
                ProductId = feedback.ProductId,
                OrderId = feedback.OrderId,
                Rating = feedback.Rating,
                Comment = feedback.Comment,
                Reply = feedback.Reply,
                ReplyDate = feedback.ReplyDate,
                CreatedDate = feedback.CreatedAt,
                UpdatedAt = feedback.UpdatedAt,
                MediaFiles = feedback.FeedbackImages
                    .Select(img => new FeedbackMediaResponseDTO
                    {
                        MediaId = img.ImageId,
                        MediaUrl = img.ImageUrl,
                        PublicId = img.PublicId,
                        MediaType = DetectMediaType(img.ImageUrl)
                    })
                    .ToList()
            };
        }
        private static string DetectMediaType(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "image";

            if (url.Contains("/video/upload/", StringComparison.OrdinalIgnoreCase))
                return "video";

            if (url.Contains("/image/upload/", StringComparison.OrdinalIgnoreCase))
                return "image";

            var extension = Path.GetExtension(url).ToLowerInvariant();
            return VideoExtensions.Contains(extension) ? "video" : "image";
        }

        private async Task<List<FeedbackImage>> UploadMediaFilesAsync(
    List<IFormFile> files, Guid feedbackId)
        {
            var mediaList = new List<FeedbackImage>();

            var imageFiles = new List<IFormFile>();
            var videoFiles = new List<IFormFile>();

            foreach (var file in files)
            {
                var contentType = file.ContentType.ToLower();
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (contentType.StartsWith("image/") && AllowedImageExtensions.Contains(ext))
                    imageFiles.Add(file);
                else if (contentType.StartsWith("video/") && AllowedVideoExtensions.Contains(ext))
                    videoFiles.Add(file);
            }

            var imagesToProcess = imageFiles.Take(MaxImagesPerFeedback).ToList();
            var videosToProcess = videoFiles.Take(MaxVideosPerFeedback).ToList();

            foreach (var file in imagesToProcess)
            {
                if (file.Length > MaxImageSizeBytes) continue;

                var uploadResult = await _cloudinaryService.UploadImageAsync(file, "feedbacks");
                if (uploadResult?.StatusCode != System.Net.HttpStatusCode.OK) continue;

                mediaList.Add(new FeedbackImage
                {
                    ImageId = Guid.NewGuid(),
                    FeedbackId = feedbackId,
                    ImageUrl = uploadResult.SecureUrl.ToString(),
                    PublicId = uploadResult.PublicId,
                    IsActive = true
                });
            }

            foreach (var file in videosToProcess)
            {
                if (file.Length > MaxVideoSizeBytes) continue;

                var uploadResult = await _cloudinaryService.UploadVideoAsync(file, "feedbacks");
                if (uploadResult?.StatusCode != System.Net.HttpStatusCode.OK) continue;

                mediaList.Add(new FeedbackImage
                {
                    ImageId = Guid.NewGuid(),
                    FeedbackId = feedbackId,
                    ImageUrl = uploadResult.SecureUrl.ToString(),
                    PublicId = uploadResult.PublicId,
                    IsActive = true
                });
            }

            return mediaList;
        }
    }
}
