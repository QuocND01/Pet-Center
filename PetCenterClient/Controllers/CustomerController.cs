using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

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

        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileRequestDto dto)
        {
            var result = await _customerService.UpdateProfileAsync(dto);
            if (result)
                return Json(new { success = true, message = "Profile updated successfully" });

            return Json(new { success = false, message = "Failed to update profile" });
        }
    }
}
