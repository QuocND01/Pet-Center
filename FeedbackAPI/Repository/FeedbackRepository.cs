using FeedbackAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FeedbackAPI.Repository
{
    public class FeedbackRepository
    {
        private readonly PetCenterFeedbackServiceContext _db;

        public FeedbackRepository(PetCenterFeedbackServiceContext db)
        {
            _db = db;
        }

        //Get all active feedback
        public IQueryable<ProductFeedback> GetAll()
        {
            return _db.ProductFeedbacks
                .Where(f => f.IsActive == true)
                .AsQueryable();
        }

        //Get by id
        public async Task<ProductFeedback?> GetByIdAsync(Guid id)
        {
            return await _db.ProductFeedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == id);
        }

        //Add new feedback
        public async Task AddAsync(ProductFeedback feedback)
        {
            await _db.ProductFeedbacks.AddAsync(feedback);
            await _db.SaveChangesAsync();
        }

        //Update feedback
        public async Task UpdateAsync(ProductFeedback feedback)
        {
            _db.ProductFeedbacks.Update(feedback);
            await _db.SaveChangesAsync();
        }

        //Soft delete
        public async Task SoftDeleteAsync(Guid id)
        {
            var feedback = await _db.ProductFeedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == id);

            if (feedback != null)
            {
                feedback.IsActive = false;
                await _db.SaveChangesAsync();
            }
        }

        //Toggle visibility
        public async Task ToggleVisibilityAsync(Guid id)
        {
            var feedback = await _db.ProductFeedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == id);

            if (feedback != null)
            {
                feedback.IsVisible = !feedback.IsVisible;
                await _db.SaveChangesAsync();
            }
        }
    }
}