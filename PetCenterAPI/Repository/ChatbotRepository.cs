using Microsoft.EntityFrameworkCore;
using PetCenterAPI.DTOs.Responses.ChatBot;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class ChatbotRepository : IChatbotRepository
    {
        private readonly PetCenterContext _context;

        public ChatbotRepository(PetCenterContext context)
        {
            _context = context;
        }

        // ============================================================
        // PENDING ORDERS — status 1, 2, 3 (Pending / Processing / Shipping)
        // ============================================================
        public async Task<List<ChatbotPendingOrderResponseDTO>> GetPendingOrdersAsync(Guid customerId)
            => await _context.Orders
                .Where(o => o.CustomerId == customerId
                         && (o.Status == 1 || o.Status == 2 || o.Status == 3))
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new ChatbotPendingOrderResponseDTO
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate ?? DateTime.Now,
                    TotalAmount = o.TotalAmount,
                    StatusCode = o.Status,   // fix ở đây
                    Status = o.Status == 1 ? "Pending"
           : o.Status == 2 ? "Processing"
           : "Shipping",
                    PaymentMethod = o.PaymentMethod
                })
                .ToListAsync();

        // ============================================================
        // LATEST ORDER STATUS — lấy đơn gần nhất không phân biệt status
        // ============================================================
        public async Task<ChatbotLatestOrderResponseDTO?> GetLatestOrderStatusAsync(Guid customerId)
            => await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new ChatbotLatestOrderResponseDTO
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate ?? DateTime.Now,
                    TotalAmount = o.TotalAmount,
                    StatusCode = o.Status,
                    Status = o.Status == 0 ? "Cancelled"
           : o.Status == 1 ? "Pending"
           : o.Status == 2 ? "Processing"
           : o.Status == 3 ? "Shipping"
           : "Completed",
                    PaymentMethod = o.PaymentMethod,
                    DeliveredDate = o.DeliveredDate
                })
                .FirstOrDefaultAsync();
    }
}
