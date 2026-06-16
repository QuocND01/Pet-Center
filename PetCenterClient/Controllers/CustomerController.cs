using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.CustomerProfile;

namespace PetCenterClient.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerApiService _customerService;
        private readonly IAuthApiService _authService;

        public CustomerController(ICustomerApiService customerService, IAuthApiService authService)
        {
            _customerService = customerService;
            _authService = authService;
        }

        // ============================================================
        // CUSTOMER — VIEW PROFILE
        // ============================================================
        public async Task<IActionResult> Profile()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var profile = await _customerService.GetProfileAsync();
            if (profile == null)
                return RedirectToAction("Login", "Auth");

            return View("~/Views/CustomerViews/Customer/Profile.cshtml", profile);
        }

        // ============================================================
        // CUSTOMER — UPDATE PROFILE
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileViewModel dto)
        {
            var result = await _customerService.UpdateProfileAsync(dto);

            if (!result.Success)
                return Json(new { success = false, message = result.Message });

            return Json(new { success = true, message = result.Message });
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            return View("~/Views/CustomerViews/Customer/ChangePassword.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
        {
            var result = await _authService.ChangePasswordAsync(dto);

            if (!result.Success)
                return Json(new { success = false, message = result.Message });

            return Json(new { success = true, message = result.Message });
        }
    }
}
