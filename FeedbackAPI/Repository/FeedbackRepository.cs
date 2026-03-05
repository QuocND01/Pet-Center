using FeedbackAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FeedbackAPI.Repository
{
    public class FeedbackRepository
    {
        private readonly PetCenterContext _db;

        public FeedbackRepository(PetCenterContext db)
        {
            _db = db;
        }

        public IQueryable<ProductFeedback> GetAll()
        {
            return _db.ProductFeedbacks
                .Include(f => f.Customer)
                .Include(f => f.Product)
                .Where(f => f.IsActive == true);
        }

        public Task<ProductFeedback?> GetByIdAsync(Guid id)
        {
            return _db.ProductFeedbacks
                .Include(f => f.Customer)
                .Include(f => f.Product)
                .FirstOrDefaultAsync(f => f.FeedbackId == id);
        }

        public async Task AddAsync(ProductFeedback feedback)
        {
            _db.ProductFeedbacks.Add(feedback);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductFeedback feedback)
        {
            _db.ProductFeedbacks.Update(feedback);
            await _db.SaveChangesAsync();
        }
    }
}
