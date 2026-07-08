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

        // 2. Dành cho Staff: Lấy danh sách các khách hàng đã từng chat với mình
        [HttpGet("my-customers")]
        [Authorize(Roles = "Admin,Vet,Sale Staff")]
        public async Task<IActionResult> GetMyCustomers()
        {
            var staffId = GetUserId();

            // Tìm những CustomerId đã gửi hoặc nhận tin nhắn với Staff này
            var customerIds = await _db.ChatMessages
                .Where(m => m.SenderId == staffId || m.ReceiverId == staffId)
                .Select(m => m.SenderId == staffId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            var customers = await _db.Customers
                .Where(c => customerIds.Contains(c.CustomerId))
                .Select(c => new { c.CustomerId, c.FullName, c.Email })
                .ToListAsync();

            return Ok(customers);
        }
    }
}