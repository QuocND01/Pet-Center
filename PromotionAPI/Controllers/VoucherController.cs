using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PromotionAPI.DTOs;
using PromotionAPI.Service.Interface;

namespace PromotionAPI.Controllers
{
    [Route("api/vouchers")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _service;

        public VoucherController(IVoucherService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok(await _service.GetByIdAsync(id));

        [HttpPost]
        public async Task<IActionResult> Create(CreateVoucherDTO dto)
        {
            await _service.CreateAsync(dto);
            return Ok("Created");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return Ok("Deleted");
        }

        [HttpPost("apply")]
        public async Task<IActionResult> Apply(ApplyVoucherDTO dto)
        {
            var result = await _service.ApplyVoucherAsync(dto);
            return Ok(result);
        }
    }
}
