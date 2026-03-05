using FeedbackAPI.Models;

namespace FeedbackAPI.Repository.Interface
{
    public interface IFeedbackRepository
    {
        IQueryable<ProductFeedback> GetAll();
        Task<ProductFeedback?> GetByIdAsync(Guid id);
        Task AddAsync(ProductFeedback feedback);
        Task UpdateAsync(ProductFeedback feedback);
    }
}
