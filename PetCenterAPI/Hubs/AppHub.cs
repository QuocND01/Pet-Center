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
        // 1. Lưu Staff đang Online và số lượng khách họ đang chat (để chia đều việc)
        // Key: StaffId | Value: Số lượng khách đang phục vụ
        private static readonly ConcurrentDictionary<Guid, int> OnlineStaffLoads = new();

        // 2. Lưu cặp Khách - Nhân viên (Ai đang chat với ai)
        // Key: CustomerId | Value: StaffId
        private static readonly ConcurrentDictionary<Guid, Guid> CustomerStaffMap = new();

        private readonly PetCenterContext _db;
        public AppHub(PetCenterContext db) => _db = db;

        private string GetUserId() => Context.UserIdentifier ?? "";
        private string GetUserRole() => Context.User?.FindFirstValue(ClaimTypes.Role) ?? "";

        public override async Task OnConnectedAsync()
        {
            var userId = Guid.Parse(GetUserId());
            var role = GetUserRole();

            // Nếu là Staff đăng nhập -> Đưa vào hàng chờ trực chat
            if (role == "Admin" || role == "Vet" || role == "Sale Staff")
            {
                OnlineStaffLoads.TryAdd(userId, 0); // Vừa online nên đang rảnh (load = 0)
                await Clients.Caller.SendAsync("ReceiveSystemMessage", "Bạn đã online và sẵn sàng nhận chat từ khách.");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Guid.Parse(GetUserId());
            var role = GetUserRole();

            // Nếu Staff offline -> Gỡ khỏi hàng chờ trực chat
            if (role == "Admin" || role == "Vet" || role == "Sale Staff")
            {
                OnlineStaffLoads.TryRemove(userId, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // =========================================================================
        // HÀM DÀNH CHO CUSTOMER GỬI TIN NHẮN (HỆ THỐNG TỰ ĐỘNG TÌM STAFF MATCH)
        // =========================================================================
        public async Task SendMessageToSupport(string content)
        {
            var customerId = Guid.Parse(GetUserId());
            Guid assignedStaffId = Guid.Empty;

            // 1. Kiểm tra xem Khách này đã có Staff nào đang support chưa
            if (CustomerStaffMap.TryGetValue(customerId, out Guid existingStaffId))
            {
                // Kiểm tra xem Staff này còn online không
                if (OnlineStaffLoads.ContainsKey(existingStaffId))
                {
                    assignedStaffId = existingStaffId;
                }
            }

            // 2. Nếu chưa có hoặc Staff cũ đã offline -> Tìm Staff mới rảnh nhất (Ít khách nhất)
            if (assignedStaffId == Guid.Empty)
            {
                if (OnlineStaffLoads.IsEmpty)
                {
                    await Clients.Caller.SendAsync("ReceiveSystemMessage", "Hiện tại không có nhân viên nào online. Tin nhắn của bạn đã được ghi nhận.");
                    return; // Vẫn có thể lưu DB tùy ý, nhưng ở đây báo lỗi tạm
                }

                // Thuật toán tìm Staff rảnh nhất
                assignedStaffId = OnlineStaffLoads.OrderBy(x => x.Value).First().Key;

                // Map khách này với Staff vừa tìm được
                CustomerStaffMap[customerId] = assignedStaffId;

                // Tăng số lượng khách mà Staff này đang phục vụ lên 1
                OnlineStaffLoads[assignedStaffId]++;

                // Báo cho Staff biết có khách mới
                await Clients.User(assignedStaffId.ToString()).SendAsync("NewCustomerAssigned", customerId);
            }

            // 3. Lưu tin nhắn vào Database
            var chatMsg = new ChatMessage
            {
                MessageId = Guid.NewGuid(),
                SenderId = customerId,
                ReceiverId = assignedStaffId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };
            _db.ChatMessages.Add(chatMsg);
            await _db.SaveChangesAsync();

            // 4. Bắn Real-time
            await Clients.User(assignedStaffId.ToString()).SendAsync("ReceiveMessage", customerId, content, chatMsg.CreatedAt);
            await Clients.Caller.SendAsync("ReceiveMessage", customerId, content, chatMsg.CreatedAt);
        }

        // =========================================================================
        // HÀM DÀNH CHO STAFF TRẢ LỜI KHÁCH HÀNG
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
                CreatedAt = DateTime.UtcNow
            };
            _db.ChatMessages.Add(chatMsg);
            await _db.SaveChangesAsync();

            await Clients.User(customerId.ToString()).SendAsync("ReceiveMessage", staffId, content, chatMsg.CreatedAt);
            await Clients.Caller.SendAsync("ReceiveMessage", staffId, content, chatMsg.CreatedAt);
        }
    }
}