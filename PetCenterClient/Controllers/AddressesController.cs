using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class AddressesController : Controller
    {
        private readonly IAddressAPIClient _apiClient;

        public AddressesController(IAddressAPIClient apiClient)
        {
            _apiClient = apiClient;
        }

        // Hiển thị trang giao diện
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Customer")
            {
                return RedirectToAction("Index", "Home");
            }

            var addresses = await _apiClient.GetMyAddressesAsync();
            return View("~/Views/CustomerViews/Addresses/Index.cshtml", addresses);
        }

        // Các hàm này chỉ trả về JSON để JavaScript ngoài View xử lý (không load lại trang)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MutateAddressViewModel dto)
        {
            var success = await _apiClient.AddAddressAsync(dto);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [FromBody] MutateAddressViewModel dto)
        {
            var success = await _apiClient.UpdateAddressAsync(id, dto);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _apiClient.DeleteAddressAsync(id);
            return Json(new { success });
        }
    }
}