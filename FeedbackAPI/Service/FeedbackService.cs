using AutoMapper;
using FeedbackAPI.DTOs;
using FeedbackAPI.Models;
using FeedbackAPI.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace FeedbackAPI.Service
{
    public class FeedbackService : IFeedbackService
    {
        private readonly PetCenterFeedbackServiceContext _context;
        private readonly IMapper _mapper;

        public FeedbackService(PetCenterFeedbackServiceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //Customer create feedback
        public async Task CreateFeedbackAsync(CreateFeedbackDTO dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                throw new Exception("Rating must be between 1 and 5");

            var alreadyFeedback = await _context.ProductFeedbacks
                .AnyAsync(x =>
                    x.ProductId == dto.ProductId &&
                    x.CustomerId == dto.CustomerId &&
                    x.IsActive == true);

            if (alreadyFeedback)
                throw new InvalidOperationException("You already reviewed this product");

            var feedback = _mapper.Map<ProductFeedback>(dto);

            feedback.FeedbackId = Guid.NewGuid();
            feedback.CreatedDate = DateTime.UtcNow;
            feedback.IsActive = true;
            feedback.IsVisible = true;

            await _context.ProductFeedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();
        }

        //Get feedback by product
        public async Task<List<FeedbackResponseDTO>> GetByProductAsync(Guid productId)
        {
            var list = await _context.ProductFeedbacks
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
                .Where(x =>
                    x.CustomerId == customerId &&
                    x.IsActive == true)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return _mapper.Map<List<FeedbackResponseDTO>>(list);
        }

        //Get feedback detail
        public async Task<FeedbackResponseDTO?> GetDetailAsync(Guid feedbackId)
        {
            var feedback = await _context.ProductFeedbacks
                .FirstOrDefaultAsync(x => x.FeedbackId == feedbackId);

            if (feedback == null)
                return null;

            return _mapper.Map<FeedbackResponseDTO>(feedback);
        }

        //Admin view all
        public async Task<List<FeedbackResponseDTO>> GetAllForAdminAsync()
        {
            var list = await _context.ProductFeedbacks
                .Where(x => x.IsActive == true)
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
                .Where(x => x.IsActive == true) 
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

        //Staff reply
        public async Task ReplyFeedbackAsync(Guid feedbackId, Guid staffId, string reply)
        {
            var feedback = await _context.ProductFeedbacks.FindAsync(feedbackId);

            if (feedback == null || feedback.IsActive != true)
                throw new Exception("Feedback not found");

            feedback.Reply = reply;
            feedback.StaffId = staffId;
            feedback.ReplyDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateReplyAsync(Guid feedbackId, string reply)
        {
            var feedback = await _context.ProductFeedbacks.FindAsync(feedbackId);

            if (feedback == null)
                throw new Exception("Feedback not found");

            feedback.Reply = reply;
            feedback.ReplyDate = DateTime.UtcNow;

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

        //Hide / Show feedback
        public async Task ToggleVisibilityAsync(Guid feedbackId)
        {
            var feedback = await _context.ProductFeedbacks.FindAsync(feedbackId);

            if (feedback == null)
                throw new Exception("Feedback not found");

            feedback.IsVisible = !feedback.IsVisible;

            await _context.SaveChangesAsync();
        }

        //Soft delete
        public async Task DeleteFeedbackAsync(Guid feedbackId)
        {
            var feedback = await _context.ProductFeedbacks.FindAsync(feedbackId);

            if (feedback == null)
                throw new Exception("Feedback not found");

            feedback.IsActive = false;

            feedback.Reply = null;
            feedback.ReplyDate = null;
            feedback.StaffId = null;

            await _context.SaveChangesAsync();

        }
    }
}