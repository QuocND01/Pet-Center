using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class ProductFeedbackRepository : IProductFeedbackRepository
    {
        private readonly PetCenterContext _context;

        public ProductFeedbackRepository(PetCenterContext context)
        {
            _context = context;
        }

        // ============================================================
        // FEEDBACK — VIEW BY PRODUCT (CUSTOMER SIDE)
        // ============================================================
        public async Task<List<ProductFeedback>> GetFeedbacksByProductIdAsync(Guid productId)
        {
            return await _context.ProductFeedbacks
                .Include(f => f.Customer)
                .Include(f => f.FeedbackImages.Where(m => m.IsActive == true))
                .Where(f => f.ProductId == productId && f.Status == 1)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        // ============================================================
        // FEEDBACK — VIEW BY ORDER (CUSTOMER SIDE — ORDER DETAIL POPUP)
        // ============================================================
        public async Task<List<ProductFeedback>> GetFeedbacksByOrderIdAsync(Guid orderId)
        {
            return await _context.ProductFeedbacks
                .Include(f => f.Customer)
                .Include(f => f.FeedbackImages.Where(m => m.IsActive == true))
                .Where(f => f.OrderId == orderId && f.Status == 1)
                .OrderBy(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasFeedbackForOrderAsync(Guid orderId, Guid customerId)
        {
            return await _context.ProductFeedbacks
                .AnyAsync(f => f.OrderId == orderId && f.CustomerId == customerId);
        }

        // ============================================================
        // FEEDBACK — CREATE BULK (CUSTOMER SIDE)
        // ============================================================
        public async Task<List<ProductFeedback>> CreateBulkAsync(List<ProductFeedback> feedbacks)
        {
            await _context.ProductFeedbacks.AddRangeAsync(feedbacks);
            await _context.SaveChangesAsync();
            return feedbacks;
        }

        public async Task AddMediaRangeAsync(List<FeedbackImage> mediaList)
        {
            await _context.FeedbackImages.AddRangeAsync(mediaList);
            await _context.SaveChangesAsync();
        }

        public async Task<List<FeedbackImage>> GetImagesByFeedbackIdAsync(Guid feedbackId)
        {
            return await _context.FeedbackImages
                .Where(m => m.FeedbackId == feedbackId && m.IsActive == true)
                .ToListAsync();
        }
    }
}
