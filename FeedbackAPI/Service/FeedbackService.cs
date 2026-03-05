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

        public async Task CreateFeedbackAsync(CreateFeedbackDTO dto)
        {
            var feedback = _mapper.Map<ProductFeedback>(dto);

            feedback.CreatedDate = DateTime.Now;
            feedback.IsActive = true;
            feedback.IsVisible = true;

            await _context.ProductFeedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();
        }

        public async Task<List<FeedbackResponseDTO>> GetByProductAsync(Guid productId)
        {
            var feedbacks = await _context.ProductFeedbacks
                .Include(x => x.Customer)
                .Where(x => x.ProductId == productId
                            && x.IsActive == true
                            && x.IsVisible == true)
                .ToListAsync();

            return _mapper.Map<List<FeedbackResponseDTO>>(feedbacks);
        }

        public async Task ReplyFeedbackAsync(Guid feedbackId, string reply)
        {
            var feedback = await _context.ProductFeedbacks
                .FirstOrDefaultAsync(x => x.FeedbackId == feedbackId);

            if (feedback == null)
                return;

            feedback.Reply = reply;
            feedback.ReplyDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}
