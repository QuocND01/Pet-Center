using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PetCenterAPI.Models;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace PetCenterAPI.Hubs
{
    [Authorize]
    public class AppHub : Hub
    {
        // Chỉ cần lưu Staff nào đang Online để chia tải (Load Balancing)
        private static readonly ConcurrentDictionary<Guid, int> OnlineStaffLoads = new();
        private readonly PetCenterContext _db;

        public AppHub(PetCenterContext db) => _db = db;

        private string GetUserId() => Context.UserIdentifier ?? "";
        private string GetUserRole() => Context.User?.FindFirstValue(ClaimTypes.Role) ?? "";

        public override async Task OnConnectedAsync()
        {
            var userId = Guid.Parse(GetUserId());
            var role = GetUserRole();

            // Đưa Staff vào hàng chờ nhận Chat
            if (role == "Admin" || role == "Vet" || role == "Sale Staff")
            {
                OnlineStaffLoads.TryAdd(userId, 0);
                await Clients.Caller.SendAsync("ReceiveSystemMessage", "Bạn đã online và sẵn sàng nhận chat từ khách.");
                // Add staff to Admins group so they receive order notifications
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Guid.Parse(GetUserId());
            var role = GetUserRole();

            if (role == "Admin" || role == "Vet" || role == "Sale Staff")
            {
                OnlineStaffLoads.TryRemove(userId, out _);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // =========================================================================
        // HÀM XỬ LÝ KHÁCH HÀNG GỬI TIN NHẮN (AUTO-ROUTING CHUẨN DOANH NGHIỆP)
        // =========================================================================
        public async Task SendMessageToSupport(string content)
        {
            var customerId = Guid.Parse(GetUserId());
            Guid assignedStaffId = Guid.Empty;

            // 1. TÌM NHÂN VIÊN GẦN NHẤT TRONG DATABASE
            // Quét xem trước đây khách này đã từng chat với ai chưa (Chống mất data khi Server khởi động lại)
            var lastChat = _db.ChatMessages
                .Where(m => m.SenderId == customerId || m.ReceiverId == customerId)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();

            if (lastChat != null)
            {
                assignedStaffId = lastChat.SenderId == customerId ? lastChat.ReceiverId : lastChat.SenderId;
            }

            bool isStaffOnline = OnlineStaffLoads.ContainsKey(assignedStaffId);

            // 2. THUẬT TOÁN ĐIỀU HƯỚNG (RE-ROUTE)
            // Nếu: Chưa từng chat với ai HOẶC Nhân viên cũ đang OFFLINE (Và đang có nhân viên khác ONLINE)
            if ((assignedStaffId == Guid.Empty || !isStaffOnline) && !OnlineStaffLoads.IsEmpty)
            {
                // Tìm nhân viên rảnh nhất (ít khách nhất)
                assignedStaffId = OnlineStaffLoads.OrderBy(x => x.Value).First().Key;
                OnlineStaffLoads[assignedStaffId]++; // Tăng số khách họ đang tiếp lên

                await Clients.User(assignedStaffId.ToString()).SendAsync("NewCustomerAssigned", customerId);
                isStaffOnline = true;
            }

            // 3. XỬ LÝ TRƯỜNG HỢP TOÀN BỘ CÔNG TY NGHỈ (OFFLINE HẾT)
            if (assignedStaffId == Guid.Empty && OnlineStaffLoads.IsEmpty)
            {
                // Bốc đại 1 nhân viên bất kỳ trong Database để tin nhắn có "chỗ trú"
                var fallbackStaff = _db.Staffs.FirstOrDefault(s => s.IsActive == true);
                if (fallbackStaff != null) assignedStaffId = fallbackStaff.StaffId;
            }

            // 4. THÔNG BÁO CHO KHÁCH (NẾU NHÂN VIÊN ĐANG OFFLINE)
            if (!isStaffOnline)
            {
                await Clients.Caller.SendAsync("ReceiveSystemMessage", "Hiện tại nhân viên đang offline. Tin nhắn đã được lưu và chúng tôi sẽ phản hồi sớm nhất.");
            }

            // 5. LƯU VÀO DATABASE (Đoạn này bị lỗi nuốt tin nhắn ở code cũ, nay đã được Fix)
            if (assignedStaffId != Guid.Empty)
            {
                var chatMsg = new ChatMessage
                {
                    MessageId = Guid.NewGuid(),
                    SenderId = customerId,
                    ReceiverId = assignedStaffId,
                    Content = content,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                _db.ChatMessages.Add(chatMsg);
                await _db.SaveChangesAsync();

                // 6. Bắn Real-time
                await Clients.User(assignedStaffId.ToString()).SendAsync("ReceiveMessage", customerId, content, chatMsg.CreatedAt);
                await Clients.Caller.SendAsync("ReceiveMessage", customerId, content, chatMsg.CreatedAt);
            }
        }

        // =========================================================================
        // HÀM STAFF TRẢ LỜI
        // =========================================================================
        public async Task SendMessageToCustomer(Guid customerId, string content)
        {
            var staffId = Guid.Parse(GetUserId());

            var chatMsg = new ChatMessage
            {
                MessageId = Guid.NewGuid(),
                SenderId = staffId,
                ReceiverId = customerId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _db.ChatMessages.Add(chatMsg);
            await _db.SaveChangesAsync();

            await Clients.User(customerId.ToString()).SendAsync("ReceiveMessage", staffId, content, chatMsg.CreatedAt);
            await Clients.Caller.SendAsync("ReceiveMessage", staffId, content, chatMsg.CreatedAt);
        }
    }
}