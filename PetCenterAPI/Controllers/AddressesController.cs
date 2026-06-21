using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.Service.Interface;
using System.Security.Claims;
using static PetCenterAPI.DTOs.Requests.CustomerProfile.AddressRequestDTO;

namespace PetCenterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")] // Chốt chặn bảo mật: Chỉ Customer mới được vào
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressesController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        // Hàm hỗ trợ bóc tách CustomerId từ JWT Token một cách an toàn
        private Guid GetCustomerId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(idClaim!);
        }

        // 1. LẤY DANH SÁCH ĐỊA CHỈ
        [HttpGet("my-addresses")]
        public async Task<IActionResult> GetMyAddresses()
        {
            try
            {
                var addresses = await _addressService.GetCustomerAddressesAsync(GetCustomerId());
                return Ok(addresses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // 2. THÊM ĐỊA CHỈ MỚI
        [HttpPost]
        public async Task<IActionResult> AddAddress([FromBody] MutateAddressDTO dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var success = await _addressService.AddAddressAsync(GetCustomerId(), dto);

                if (success)
                    return Ok(new { success = true, message = "Address added successfully." });

                return BadRequest(new { success = false, message = "Failed to add address." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // 3. CẬP NHẬT ĐỊA CHỈ
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] MutateAddressDTO dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var success = await _addressService.UpdateAddressAsync(GetCustomerId(), id, dto);

                if (success)
                    return Ok(new { success = true, message = "Address updated successfully." });

                return BadRequest(new { success = false, message = "Address not found or update failed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // 4. XÓA ĐỊA CHỈ
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAddress(Guid id)
        {
            try
            {
                var success = await _addressService.DeleteAddressAsync(GetCustomerId(), id);

                if (success)
                    return Ok(new { success = true, message = "Address deleted successfully." });

                return BadRequest(new { success = false, message = "Address not found or delete failed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}