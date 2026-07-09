using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using System.Security.Claims;

namespace PetCenterAPI.Controllers
{
    [Route("api/chat")]
    [ApiController]
    [Authorize]
    public class ChatAPIController : ControllerBase
    {
        private readonly PetCenterContext _db;
        public ChatAPIController(PetCenterContext db) => _db = db;

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // 1. Lấy lịch sử tin nhắn giữa 2 người
        [HttpGet("history/{partnerId:guid}")]
        public async Task<IActionResult> GetChatHistory(Guid partnerId)
        {
            var myId = GetUserId();
            var messages = await _db.ChatMessages
                .Where(m => (m.SenderId == myId && m.ReceiverId == partnerId) ||
                            (m.SenderId == partnerId && m.ReceiverId == myId))
                .OrderBy(m => m.CreatedAt) // Sắp xếp cũ -> mới
                .Select(m => new {
                    m.MessageId,
                    m.SenderId,
                    m.Content,
                    CreatedAt = m.CreatedAt.ToLocalTime()
                })
                .ToListAsync();

            return Ok(messages);
        }

        // 2. Lấy danh sách khách hàng (Có kèm tin nhắn cuối và trạng thái chưa đọc)
        [HttpGet("my-customers")]
        [Authorize(Roles = "Admin,Vet,Sale Staff")]
        public async Task<IActionResult> GetMyCustomers()
        {
            var staffId = GetUserId();

            var messages = await _db.ChatMessages
                .Where(m => m.SenderId == staffId || m.ReceiverId == staffId)
                .ToListAsync();

            var customerIds = messages
                .Select(m => m.SenderId == staffId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToList();

            var customers = await _db.Customers
                .Where(c => customerIds.Contains(c.CustomerId))
                .ToListAsync();

            var result = new List<object>();

            foreach (var c in customers)
            {
                var chatHistory = messages.Where(m => m.SenderId == c.CustomerId || m.ReceiverId == c.CustomerId)
                                          .OrderByDescending(m => m.CreatedAt).ToList();
                var lastMsg = chatHistory.FirstOrDefault();

                // Đếm tin nhắn KHÁCH gửi mà MÌNH chưa đọc
                var unreadCount = chatHistory.Count(m => m.SenderId == c.CustomerId && !m.IsRead);

                result.Add(new
                {
                    CustomerId = c.CustomerId,
                    FullName = c.FullName,
                    Email = c.Email,
                    LastMessage = lastMsg?.Content ?? "",
                    LastMessageTime = lastMsg?.CreatedAt.ToLocalTime(),
                    UnreadCount = unreadCount
                });
            }

            // Sắp xếp ai vừa nhắn lên đầu tiên
            var sortedResult = result.OrderByDescending(x => (x as dynamic).LastMessageTime).ToList();

            return Ok(sortedResult);
        }

        // THÊM MỚI: API đánh dấu đã đọc
        [HttpPost("mark-read/{customerId:guid}")]
        [Authorize(Roles = "Admin,Vet,Sale Staff")]
        public async Task<IActionResult> MarkAsRead(Guid customerId)
        {
            var staffId = GetUserId();
            var unreadMsgs = await _db.ChatMessages
                .Where(m => m.SenderId == customerId && m.ReceiverId == staffId && !m.IsRead)
                .ToListAsync();

            if (unreadMsgs.Any())
            {
                foreach (var msg in unreadMsgs) msg.IsRead = true;
                await _db.SaveChangesAsync();
            }
            return Ok();
        }

        // 3. Dành cho Khách hàng: Lấy toàn bộ lịch sử chat của mình
        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyHistory()
        {
            var myId = GetUserId();
            var messages = await _db.ChatMessages
                .Where(m => m.SenderId == myId || m.ReceiverId == myId)
                .OrderBy(m => m.CreatedAt) // Sắp xếp cũ -> mới
                .Select(m => new {
                    m.MessageId,
                    m.SenderId,
                    m.Content,
                    CreatedAt = m.CreatedAt.ToLocalTime()
                })
                .ToListAsync();

            return Ok(messages);
        }
    }
}