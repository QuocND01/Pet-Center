using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class VoucherController : Controller
    {
        private readonly IVoucherService _service;

        public VoucherController(IVoucherService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var vouchers = await _service.GetAllAsync();
            return View("~/Views/AdminViews/Voucher/Index.cshtml", vouchers);
        }

        public IActionResult Create()
        {
            return View("~/Views/AdminViews/Voucher/Create.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateVoucherDTO dto)
        {
            await _service.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

 
        [HttpPost]
        public async Task<IActionResult> Apply(string code, decimal orderAmount)
        {
            var customerId = GetUserId();

            var result = await _service.ApplyVoucherAsync(new ApplyVoucherDTO
            {
                CustomerId = customerId,
                Code = code,
                OrderAmount = orderAmount
            });

            return Json(result);
        }

        private Guid GetUserId()
        {
            var token = HttpContext.Session.GetString("JWT");

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var userId = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            return Guid.Parse(userId);
        }
    }
}