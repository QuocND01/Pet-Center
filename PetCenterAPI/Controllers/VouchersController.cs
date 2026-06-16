using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Controllers
{
    [ApiController]
    [Route("api/voucher")]
    public class VouchersController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VouchersController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        // ============================================================
        // VOUCHER — VIEW LIST
        // ============================================================
        [HttpGet("vouchers")]
        [Authorize(Roles = "Admin,Sale Staff")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _voucherService.GetAllAsync();
            return Ok(result);
        }
    }
}
