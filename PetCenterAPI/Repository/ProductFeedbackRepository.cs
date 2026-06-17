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
    }
}
