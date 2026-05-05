using FeedbackAPI.DTOs;
using FeedbackAPI.Models;
using FeedbackAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FeedbackAPI.Repository
{
    public class AdminFeedbackRepository : IAdminFeedbackRepository
    {
        private readonly PetCenterFeedbackServiceContext _context;

        public AdminFeedbackRepository(PetCenterFeedbackServiceContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ProductFeedback>> GetAllAsync(FeedbackFilterDto filter)
        {
            var query = _context.ProductFeedbacks
                .Where(f => f.IsActive == true)
                .AsQueryable();

            // ── Filters ───────────────────────────────────────────
            if (filter.Rating.HasValue)
                query = query.Where(f => f.Rating == filter.Rating.Value);

            if (filter.HasReply.HasValue)
                query = filter.HasReply.Value
                    ? query.Where(f => f.Reply != null && f.Reply != "")
                    : query.Where(f => f.Reply == null || f.Reply == "");

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                query = query.Where(f => f.Comment != null &&
                    f.Comment.ToLower().Contains(filter.Keyword.ToLower()));

            // ── Sorting ───────────────────────────────────────────
            query = filter.SortBy switch
            {
                "oldest" => query.OrderBy(f => f.CreatedDate),
                "rating_asc" => query.OrderBy(f => f.Rating),
                "rating_desc" => query.OrderByDescending(f => f.Rating),
                _ => query.OrderByDescending(f => f.CreatedDate) // newest default
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<ProductFeedback>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<ProductFeedback?> GetByIdAsync(Guid feedbackId)
        {
            return await _context.ProductFeedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId && f.IsActive == true);
        }

        public async Task<bool> ReplyAsync(Guid feedbackId, Guid staffId, string replyContent)
        {
            var feedback = await GetByIdAsync(feedbackId);
            if (feedback == null) return false;

            feedback.Reply = replyContent;
            feedback.StaffId = staffId;
            feedback.ReplyDate = DateTime.Now;
            feedback.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateReplyAsync(Guid feedbackId, string replyContent)
        {
            var feedback = await GetByIdAsync(feedbackId);
            if (feedback == null || string.IsNullOrEmpty(feedback.Reply))
                return false;

            feedback.Reply = replyContent;
            feedback.ReplyDate = DateTime.Now;
            feedback.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReplyAsync(Guid feedbackId)
        {
            var feedback = await GetByIdAsync(feedbackId);
            if (feedback == null) return false;

            feedback.Reply = null;
            feedback.StaffId = null;
            feedback.ReplyDate = null;
            feedback.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleVisibilityAsync(Guid feedbackId, bool isVisible)
        {
            var feedback = await GetByIdAsync(feedbackId);
            if (feedback == null) return false;

            feedback.IsVisible = isVisible;
            feedback.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
