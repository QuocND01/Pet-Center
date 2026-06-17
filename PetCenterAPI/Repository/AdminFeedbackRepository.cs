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
    }
}
