using Microsoft.EntityFrameworkCore;
using PetCenterAPI.DTOs.Requests.ManageFeedback;
using PetCenterAPI.DTOs.Responses.ManageFeedback;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class AdminFeedbackRepository : IAdminFeedbackRepository
    {
        private readonly PetCenterContext _context;

        public AdminFeedbackRepository(PetCenterContext context)
        {
            _context = context;
        }

        // ============================================================
        // FEEDBACK — VIEW LIST (ADMIN/STAFF)
        // ============================================================
        public async Task<PagedResult<AdminFeedbackItemResponseDTO>> GetAllAsync(FeedbackFilterRequestDTO filter)
        {
            var query = _context.ProductFeedbacks.AsQueryable();

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
                "oldest" => query.OrderBy(f => f.CreatedAt),
                "rating_asc" => query.OrderBy(f => f.Rating),
                "rating_desc" => query.OrderByDescending(f => f.Rating),
                _ => query.OrderByDescending(f => f.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            // ── Join directly with Customer, Product, Staff, no HTTP enrichment needed ──
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(f => new AdminFeedbackItemResponseDTO
                {
                    FeedbackId = f.FeedbackId,
                    CustomerId = f.CustomerId,
                    CustomerName = f.Customer.FullName,
                    CustomerEmail = f.Customer.Email,
                    ProductId = f.ProductId,
                    ProductName = f.Product.ProductName,
                    ProductImage = f.Product.ProductImages
                        .Where(pi => pi.IsActive == true)
                        .Select(pi => pi.ImageUrl)
                        .FirstOrDefault(),
                    OrderId = f.OrderId,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    ReplyContent = f.Reply,
                    HasReply = !string.IsNullOrEmpty(f.Reply),
                    StaffId = f.StaffId,
                    StaffName = f.Staff != null ? f.Staff.FullName : null,
                    ReplyDate = f.ReplyDate,
                    CreatedDate = f.CreatedAt,
                    IsVisible = f.Status == 1
                })
                .ToListAsync();

            return new PagedResult<AdminFeedbackItemResponseDTO>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        // ============================================================
        // FEEDBACK — VIEW DETAIL (ADMIN/STAFF)
        // ============================================================
        public async Task<AdminFeedbackItemResponseDTO?> GetByIdAsync(Guid feedbackId)
        {
            return await _context.ProductFeedbacks
                .Where(f => f.FeedbackId == feedbackId)
                .Select(f => new AdminFeedbackItemResponseDTO
                {
                    FeedbackId = f.FeedbackId,
                    CustomerId = f.CustomerId,
                    CustomerName = f.Customer.FullName,
                    CustomerEmail = f.Customer.Email,
                    ProductId = f.ProductId,
                    ProductName = f.Product.ProductName,
                    ProductImage = f.Product.ProductImages
                        .Where(pi => pi.IsActive == true)
                        .Select(pi => pi.ImageUrl)
                        .FirstOrDefault(),
                    OrderId = f.OrderId,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    ReplyContent = f.Reply,
                    HasReply = !string.IsNullOrEmpty(f.Reply),
                    StaffId = f.StaffId,
                    StaffName = f.Staff != null ? f.Staff.FullName : null,
                    ReplyDate = f.ReplyDate,
                    CreatedDate = f.CreatedAt,
                    IsVisible = f.Status == 1,

                    MediaFiles = f.FeedbackImages
                        .Where(img => img.IsActive == true)
                        .Select(img => new FeedbackMediaItemDTO
                        {
                            MediaId = img.ImageId,
                            MediaUrl = img.ImageUrl,
                            PublicId = img.PublicId,
                            MediaType = img.ImageUrl.Contains("/video/upload/", StringComparison.OrdinalIgnoreCase)
                                ? "video"
                                : img.ImageUrl.Contains("/image/upload/", StringComparison.OrdinalIgnoreCase)
                                    ? "image"
                                    : (img.ImageUrl.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                                       img.ImageUrl.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
                                       img.ImageUrl.EndsWith(".webm", StringComparison.OrdinalIgnoreCase)
                                        ? "video" : "image")
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        // ============================================================
        // FEEDBACK — REPLY
        // ============================================================
        public async Task<bool> ReplyAsync(Guid feedbackId, Guid staffId, string replyContent)
        {
            var feedback = await _context.ProductFeedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);

            if (feedback == null) return false;

            feedback.Reply = replyContent;
            feedback.StaffId = staffId;
            feedback.ReplyDate = DateTime.Now;
            feedback.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // FEEDBACK — UPDATE REPLY
        // ============================================================
        public async Task<bool> UpdateReplyAsync(Guid feedbackId, string replyContent)
        {
            var feedback = await _context.ProductFeedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);

            if (feedback == null || string.IsNullOrEmpty(feedback.Reply))
                return false;

            feedback.Reply = replyContent;
            feedback.ReplyDate = DateTime.Now;
            feedback.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // FEEDBACK — DELETE REPLY
        // ============================================================
        public async Task<bool> DeleteReplyAsync(Guid feedbackId)
        {
            var feedback = await _context.ProductFeedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);

            if (feedback == null) return false;

            feedback.Reply = null;
            feedback.StaffId = null;
            feedback.ReplyDate = null;
            feedback.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // FEEDBACK — TOGGLE VISIBILITY
        // ============================================================
        public async Task<bool> ToggleVisibilityAsync(Guid feedbackId, bool isVisible)
        {
            var feedback = await _context.ProductFeedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);

            if (feedback == null) return false;

            feedback.Status = isVisible ? 1 : 0;
            feedback.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
