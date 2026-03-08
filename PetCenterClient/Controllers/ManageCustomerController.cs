using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    }
}
