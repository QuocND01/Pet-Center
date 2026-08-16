using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Service.Interface;
using System.Security.Claims;

namespace PetCenterAPI.Controllers
{
    [Route("api/chat")]
    [ApiController]
    [Authorize]
    public class ChatAPIController : ControllerBase
    {
        private readonly PetCenterContext _db;
        private readonly IOrderService _orderService;

        public ChatAPIController(PetCenterContext db, IOrderService orderService)
        {
            _db = db;
            _orderService = orderService;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("history/{partnerId:guid}")]
        public async Task<IActionResult> GetChatHistory(Guid partnerId)
        {
            var myId = GetUserId();
            var messages = await _db.ChatMessages
                .Where(m => (m.SenderId == myId && m.ReceiverId == partnerId) ||
                            (m.SenderId == partnerId && m.ReceiverId == myId))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new {
                    m.MessageId,
                    m.SenderId,
                    m.Content,
                    CreatedAt = m.CreatedAt.ToLocalTime()
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpGet("my-customers")]
        [Authorize(Roles = "Admin,Vet,Sale Staff,Groomer")]
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

            var sortedResult = result.OrderByDescending(x => (x as dynamic).LastMessageTime).ToList();

            return Ok(sortedResult);
        }

        [HttpPost("mark-read/{customerId:guid}")]
        [Authorize(Roles = "Admin,Vet,Sale Staff,Groomer")]
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

        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyHistory()
        {
            var myId = GetUserId();
            var messages = await _db.ChatMessages
                .Where(m => m.SenderId == myId || m.ReceiverId == myId)
                .OrderBy(m => m.CreatedAt) 
                .Select(m => new {
                    m.MessageId,
                    m.SenderId,
                    m.Content,
                    CreatedAt = m.CreatedAt.ToLocalTime()
                })
                .ToListAsync();

            return Ok(messages);
        }

        /// <summary>
        /// Endpoint: GET /api/chat/my-orders-with-items
        /// </summary>
        [HttpGet("my-orders-with-items")]
        public async Task<IActionResult> GetMyOrdersWithItems()
        {
            try
            {
                var customerId = GetUserId();
                var orders = await _orderService.GetCustomerOrderHistoryWithItemsAsync(customerId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint: GET /api/chat/my-orders-search?keyword=...
        /// </summary>
        [HttpGet("my-orders-search")]
        public async Task<IActionResult> SearchMyOrdersByProductName([FromQuery] string keyword)
        {
            try
            {
                var customerId = GetUserId();
                if (string.IsNullOrWhiteSpace(keyword))
                    return BadRequest(new { success = false, message = "Keyword is required." });

                var orders = await _orderService.SearchCustomerOrdersByProductNameAsync(customerId, keyword);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint: GET /api/chat/my-appointments
        /// </summary>
        [HttpGet("my-appointments")]
        public async Task<IActionResult> GetMyAppointments()
        {
            try
            {
                var customerId = GetUserId();
                var appointments = await _db.Appointments
                    .Where(a => a.CustomerId == customerId)
                    .Include(a => a.Pet)
                    .Include(a => a.Staff)
                    .Include(a => a.AppointmentServices)
                        .ThenInclude(aps => aps.Service)
                    .OrderByDescending(a => a.AppointmentStart)
                    .Select(a => new
                    {
                        a.AppointmentId,
                        a.AppointmentStart,
                        a.AppointmentEnd,
                        a.Status,
                        StatusName = a.Status == 2 ? "Confirmed" : (a.Status == 1 ? "Reserved" : (a.Status == 3 ? "Completed" : "Cancelled")),
                        PetName = a.Pet != null ? a.Pet.PetName : "",
                        StaffName = a.Staff != null ? a.Staff.FullName : "",
                        ServiceNames = a.AppointmentServices.Select(s => s.Service.ServiceName).ToList(),
                        a.Total,
                        a.Note
                    })
                    .ToListAsync();

                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}