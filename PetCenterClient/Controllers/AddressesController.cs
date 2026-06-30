using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using Microsoft.Extensions.Logging;

namespace PetCenterClient.Controllers
{
    [Route("[controller]")]
    public class AddressesController : Controller
    {
        private readonly IAddressAPIClient _apiClient;
        private readonly ILogger<AddressesController> _logger;

        public AddressesController(IAddressAPIClient apiClient, ILogger<AddressesController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        /// <summary>
        /// Hiển thị trang giao diện quản lý địa chỉ của khách hàng
        /// </summary>
        /// <returns>View Index cùng với danh sách địa chỉ</returns>
        [HttpGet("Index")]
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var role = HttpContext.Session.GetString("Role");
                if (string.IsNullOrEmpty(role) || role != "Customer")
                {
                    _logger.LogWarning("Truy cập trái phép vào trang Addresses. Bắt buộc role Customer. Chuyển hướng về trang chủ.");
                    return RedirectToAction("Index", "Home");
                }

                _logger.LogInformation("Đang lấy danh sách địa chỉ cho người dùng hiện tại.");
                var addresses = await _apiClient.GetMyAddressesAsync();

                if (addresses == null)
                {
                    _logger.LogWarning("Không tìm thấy dữ liệu địa chỉ hoặc API trả về null.");
                    addresses = new List<ReadAddressViewModel>(); // Tránh lỗi NullReferenceException ở View
                }

                return View("~/Views/CustomerViews/Addresses/Index.cshtml", addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi tải trang Index của AddressesController.");
                // Trong thực tế có thể trả về một trang báo lỗi chung
                return View("Error");
            }
        }

        /// <summary>
        /// Thêm mới một địa chỉ
        /// </summary>
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] MutateAddressViewModel dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi tạo địa chỉ mới.");
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                var success = await _apiClient.AddAddressAsync(dto);
                if (success)
                {
                    _logger.LogInformation("Tạo địa chỉ mới thành công.");
                }
                else
                {
                    _logger.LogWarning("Thêm địa chỉ thất bại từ phía API Client.");
                }

                return Json(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception khi gọi API thêm địa chỉ.");
                return Json(new { success = false, message = "Đã xảy ra lỗi hệ thống" });
            }
        }

        /// <summary>
        /// Cập nhật thông tin địa chỉ hiện có
        /// </summary>
        [HttpPost("Edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] MutateAddressViewModel dto)
        {
            if (id == Guid.Empty)
            {
                return Json(new { success = false, message = "ID không hợp lệ" });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu cập nhật không hợp lệ" });
            }

            try
            {
                var success = await _apiClient.UpdateAddressAsync(id, dto);
                _logger.LogInformation($"Cập nhật địa chỉ {id} - Kết quả: {success}");
                return Json(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception khi cập nhật địa chỉ ID: {id}");
                return Json(new { success = false, message = "Đã xảy ra lỗi hệ thống" });
            }
        }

        /// <summary>
        /// Xóa địa chỉ khỏi hệ thống
        /// </summary>
        [HttpPost("Delete/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Json(new { success = false, message = "ID không hợp lệ" });
            }

            try
            {
                var success = await _apiClient.DeleteAddressAsync(id);
                _logger.LogInformation($"Xóa địa chỉ {id} - Kết quả: {success}");
                return Json(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception khi xóa địa chỉ ID: {id}");
                return Json(new { success = false, message = "Đã xảy ra lỗi hệ thống" });
            }
        }
    }
}