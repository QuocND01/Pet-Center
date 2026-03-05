using AutoMapper;
using FeedbackAPI.DTOs;
using FeedbackAPI.Models;
using FeedbackAPI.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace FeedbackAPI.Service
{
    public class FeedbackService : IFeedbackService
    {
        private readonly PetCenterContext _context;
        private readonly IMapper _mapper;

        public FeedbackService(PetCenterContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //Customer create feedback, only one, after purchased, rating 1 - 5
        public async Task CreateFeedbackAsync(CreateFeedbackDTO dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                throw new Exception("Rating must be between 1 and 5");

            var hasPurchased = await _context.OrderDetails
                .Include(x => x.Order)
                .AnyAsync(x =>
                    x.ProductId == dto.ProductId &&
                    x.Order.CustomerId == dto.CustomerId &&
                    x.OrderId == dto.OrderId &&
                    x.Order.Status == 1);

            if (!hasPurchased)
                throw new Exception("Customer has not purchased this product");
            
            var alreadyFeedback = await _context.ProductFeedbacks
               .AnyAsync(x =>
                        x.ProductId == dto.ProductId &&
                        x.CustomerId == dto.CustomerId &&
                        x.IsActive == true);

            if (alreadyFeedback)
                throw new InvalidOperationException("You already reviewed this product");

            var feedback = _mapper.Map<ProductFeedback>(dto);

            feedback.CreatedDate = DateTime.Now;
            feedback.IsActive = true;
            feedback.IsVisible = true;

            await _context.ProductFeedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();
        }

        //Get feedback by product (only active)
        public async Task<List<FeedbackResponseDTO>> GetByProductAsync(Guid productId)
        {
            var list = await _context.ProductFeedbacks
                .Include(x => x.Customer)
                .Where(x =>
                    x.ProductId == productId &&
                    x.IsActive == true &&
                    x.IsVisible == true)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return _mapper.Map<List<FeedbackResponseDTO>>(list);
        }

        //Get feedback by customer
        public async Task<List<FeedbackResponseDTO>> GetByCustomerAsync(Guid customerId)
        {
            var list = await _context.ProductFeedbacks
                .Include(x => x.Product)
                .Where(x =>
                    x.CustomerId == customerId &&
                    x.IsActive == true)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return _mapper.Map<List<FeedbackResponseDTO>>(list);
        }

        //Get feedback detail (for admin, staff)
        public async Task<FeedbackResponseDTO?> GetDetailAsync(Guid feedbackId)
        {
            var feedback = await _context.ProductFeedbacks
                .Include(x => x.Customer)
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.FeedbackId == feedbackId);

            return feedback == null
                ? null
                : _mapper.Map<FeedbackResponseDTO>(feedback);
        }


        //View all feedback for admin (including hidden, not active)
        public async Task<List<FeedbackResponseDTO>> GetAllForAdminAsync()
        {
            var list = await _context.ProductFeedbacks
                .Include(x => x.Customer)
                .Include(x => x.Product)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return _mapper.Map<List<FeedbackResponseDTO>>(list);
        }

        //Filter feedback
        public async Task<List<FeedbackResponseDTO>> FilterAsync(
            int? rating,
            Guid? productId,
            bool? isVisible,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.ProductFeedbacks
                .Include(x => x.Customer)
                .Include(x => x.Product)
                .AsQueryable();

            if (rating.HasValue)
                query = query.Where(x => x.Rating == rating);

            if (productId.HasValue)
                query = query.Where(x => x.ProductId == productId);

            if (isVisible.HasValue)
                query = query.Where(x => x.IsVisible == isVisible);

            if (fromDate.HasValue)
                query = query.Where(x => x.CreatedDate >= fromDate);

            if (toDate.HasValue)
                query = query.Where(x => x.CreatedDate <= toDate);

            var list = await query
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return _mapper.Map<List<FeedbackResponseDTO>>(list);
        }

        //Staff reply to feedback, update reply, delete reply
        public async Task ReplyFeedbackAsync(Guid feedbackId, Guid staffId, string reply)
        {
            var feedback = await _context.ProductFeedbacks.FindAsync(feedbackId);

            if (feedback == null || feedback.IsActive != true)
                throw new Exception("Feedback not found");

            feedback.Reply = reply;
            feedback.StaffId = staffId;
            feedback.ReplyDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateReplyAsync(Guid feedbackId, string reply)
        {
            var feedback = await _context.ProductFeedbacks.FindAsync(feedbackId);

            if (feedback == null)
                throw new Exception("Feedback not found");

            feedback.Reply = reply;
            feedback.ReplyDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }


        public async Task DeleteReplyAsync(Guid feedbackId)
        {
            var feedback = await _context.ProductFeedbacks.FindAsync(feedbackId);

            if (feedback == null)
                throw new Exception("Feedback not found");

            feedback.Reply = null;
            feedback.ReplyDate = null;
            feedback.StaffId = null;

            await _context.SaveChangesAsync();
        }


        //Hide/show feedback
        public async Task ToggleVisibilityAsync(Guid feedbackId)
        {
            var feedback = await _context.ProductFeedbacks.FindAsync(feedbackId);

            if (feedback == null)
                throw new Exception("Feedback not found");

            feedback.IsVisible = !(feedback.IsVisible ?? true);

            await _context.SaveChangesAsync();
        }

        //Delete feedback (soft delete)
        public async Task DeleteFeedbackAsync(Guid feedbackId)
        {
            var feedback = await _context.ProductFeedbacks.FindAsync(feedbackId);

            if (feedback == null)
                throw new Exception("Feedback not found");

            feedback.IsActive = false;

            await _context.SaveChangesAsync();
        }
    }
}
