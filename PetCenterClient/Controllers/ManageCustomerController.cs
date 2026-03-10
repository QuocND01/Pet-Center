using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class ManageCustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public ManageCustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("AdminLogin", "Auth");

            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Staff") return RedirectToAction("AdminLogin", "Auth");

            var customers = await _customerService.GetAllCustomersAsync();
            return View("~/Views/AdminViews/ManageCustomer/Index.cshtml", customers);
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("AdminLogin", "Auth");

            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Staff") return RedirectToAction("AdminLogin", "Auth");

            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();
            return View("~/Views/AdminViews/ManageCustomer/Detail.cshtml", customer);
        }

        // ===== SEARCH API ENDPOINT =====
        [HttpGet]
        [Route("ManageCustomer/Search")]
        public async Task<IActionResult> Search(string query)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return Json(new { success = false, message = "Unauthorized" });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Staff")
                return Json(new { success = false, message = "Forbidden" });

            try
            {
                // Nếu query trống, lấy tất cả
                if (string.IsNullOrEmpty(query))
                {
                    var allCustomers = await _customerService.GetAllCustomersAsync();
                    return Json(new { success = true, data = allCustomers });
                }

                // Lấy tất cả customers
                var customers = await _customerService.GetAllCustomersAsync();

                // Normalize search query - Remove diacritics
                var normalizedQuery = RemoveDiacritics(query.ToLower());

                // Search: match tên và email (normalized)
                var results = customers.Where(c =>
                    RemoveDiacritics(c.FullName.ToLower()).Contains(normalizedQuery) ||
                    c.Email.ToLower().Contains(query.ToLower())
                ).ToList();

                return Json(new { success = true, data = results });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ===== HELPER: Remove Diacritics for Vietnamese search =====
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        // Thêm vào class
        [HttpPut]
        [Route("ManageCustomer/ChangeStatus/{customerId:guid}")]
        public async Task<IActionResult> ChangeStatus(Guid customerId, [FromBody] ChangeCustomerStatusRequestDto request)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return Json(new { success = false, message = "Unauthorized" });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
                return Json(new { success = false, message = "Only Admin can change customer status" });

            try
            {
                var result = await _customerService.ChangeCustomerStatusAsync(customerId, request.IsActive);
                if (result)
                    return Json(new { success = true, message = "Status changed successfully" });
                else
                    return Json(new { success = false, message = "Customer not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
