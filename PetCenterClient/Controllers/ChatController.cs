using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PetCenterClient.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatAPIClient _chatApi;

        public ChatController(IChatAPIClient chatApi)
        {
            _chatApi = chatApi;
        }

        // 🚀 HÀM HỖ TRỢ BÓC TÁCH USER ID TRỰC TIẾP TỪ JWT TOKEN
        private string GetUserIdFromToken()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token)) return "";

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Tìm Claim chứa ID (NameIdentifier)
            var userId = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type == "nameid" ||
                c.Type == "sub")?.Value;

            return userId ?? "";
        }

        public IActionResult CustomerChat()
        {
            // (Bạn có thể đổi "Login", "Auth" thành đường dẫn thực tế trang đăng nhập của bạn)
            if (HttpContext.Session.GetString("Role") != "Customer")
                return RedirectToAction("Login", "Auth");

            // Sử dụng hàm bóc tách ID siêu chuẩn
            ViewBag.MyId = GetUserIdFromToken();
            return View("~/Views/Chat/CustomerChat.cshtml");
        }

        public async Task<IActionResult> StaffChat()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Vet" && role != "Admin" && role != "Sale Staff")
                return RedirectToAction("AdminLogin", "Auth");

            // Sử dụng hàm bóc tách ID siêu chuẩn
            ViewBag.MyId = GetUserIdFromToken();

            // Lấy danh sách khách hàng đang được Staff này hỗ trợ
            var customers = await _chatApi.GetMyCustomersAsync();
            return View("~/Views/Chat/StaffChat.cshtml", customers);
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory(Guid id)
        {
            var history = await _chatApi.GetChatHistoryAsync(id);
            return Json(history);
        }


    }
}