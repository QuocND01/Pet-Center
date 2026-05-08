using FeedbackAPI.Models;
using FeedbackAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FeedbackAPI.Repository
{
    public class ProductFeedbackRepository : IProductFeedbackRepository
    {
        private readonly PetCenterFeedbackServiceContext _context;

        public ProductFeedbackRepository(PetCenterFeedbackServiceContext context)
        {
            _context = context;
        }

        public async Task<List<ProductFeedback>> GetFeedbacksByProductIdAsync(Guid productId)
        {
            return await _context.ProductFeedbacks
                .Where(f => f.ProductId == productId
                         && f.IsActive == true
                         && f.IsVisible == true)
                .OrderByDescending(f => f.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<ProductFeedback>> GetFeedbacksByOrderIdAsync(Guid orderId)
        {
            return await _context.ProductFeedbacks
                .Where(f => f.OrderId == orderId
                         && f.IsActive == true
                         && f.IsVisible == true)
                .OrderBy(f => f.CreatedDate)
                .ToListAsync();
        }

        public async Task<ProductFeedback?> GetFeedbackByIdAsync(Guid feedbackId)
        {
            return await _context.ProductFeedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId
                                       && f.IsActive == true);
        }

        public async Task<bool> HasFeedbackForOrderAsync(Guid orderId, Guid customerId)
        {
            return await _context.ProductFeedbacks
                .AnyAsync(f => f.OrderId == orderId
                            && f.CustomerId == customerId
                            && f.IsActive == true);
        }

        public async Task<List<ProductFeedback>> CreateBulkFeedbackAsync(List<ProductFeedback> feedbacks)
        {
            await _context.ProductFeedbacks.AddRangeAsync(feedbacks);
            await _context.SaveChangesAsync();
            return feedbacks;
        }

        public async Task<ProductFeedback?> UpdateFeedbackAsync(ProductFeedback feedback)
        {
            var existing = await _context.ProductFeedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == feedback.FeedbackId
                                       && f.IsActive == true);

            if (existing == null) return null;

            existing.Rating = feedback.Rating;
            existing.Comment = feedback.Comment;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return existing;
        }
    }
}
