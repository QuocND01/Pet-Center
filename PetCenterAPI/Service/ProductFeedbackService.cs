using PetCenterAPI.DTOs.Responses.ManageFeedback;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class ProductFeedbackService : IProductFeedbackService
    {
        private readonly IProductFeedbackRepository _productFeedbackRepository;

        private static readonly string[] VideoExtensions =
            { ".mp4", ".mov", ".webm", ".avi", ".mkv" };

        public ProductFeedbackService(IProductFeedbackRepository productFeedbackRepository)
        {
            _productFeedbackRepository = productFeedbackRepository;
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


    }
}
