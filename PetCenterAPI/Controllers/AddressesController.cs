using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;

namespace PetCenterAPI.Controllers
{
    [Route("address-service/[controller]")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        private readonly PetCenterContext _db;

        public AddressesController(PetCenterContext db)
        {
            _db = db;
        }

        // GET: address-service/Addresses/customer/{customerId}
        // Trả về danh sách địa chỉ active của customer, default lên đầu
        [HttpGet("customer/{customerId:guid}")]
        public async Task<IActionResult> GetByCustomerId(Guid customerId)
        {
            var addresses = await _db.Addresses
                .Where(a => a.CustomerId == customerId && a.IsActive == true)
                .OrderByDescending(a => a.IsDefault)
                .Select(a => new
                {
                    a.AddressId,
                    a.CustomerId,
                    a.Province,
                    a.District,
                    a.Ward,
                    a.AddressDetails,
                    IsDefault = a.IsDefault ?? false,
                    IsActive = a.IsActive ?? false
                })
                .ToListAsync();

            return Ok(addresses);
        }

        // GET: address-service/Addresses/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var address = await _db.Addresses
                .Where(a => a.AddressId == id)
                .Select(a => new
                {
                    a.AddressId,
                    a.CustomerId,
                    a.Province,
                    a.District,
                    a.Ward,
                    a.AddressDetails,
                    IsDefault = a.IsDefault ?? false,
                    IsActive = a.IsActive ?? false
                })
                .FirstOrDefaultAsync();

            if (address == null) return NotFound();
            return Ok(address);
        }

        // GET: address-service/Addresses
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var addresses = await _db.Addresses
                .Where(a => a.IsActive == true)
                .Select(a => new
                {
                    a.AddressId,
                    a.CustomerId,
                    a.Province,
                    a.District,
                    a.Ward,
                    a.AddressDetails,
                    IsDefault = a.IsDefault ?? false,
                    IsActive = a.IsActive ?? false
                })
                .ToListAsync();

            return Ok(addresses);
        }

        // POST: address-service/Addresses
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddressCreateRequest dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Nếu đây là default → bỏ default của các địa chỉ khác
            if (dto.IsDefault == true)
            {
                var existing = await _db.Addresses
                    .Where(a => a.CustomerId == dto.CustomerId && a.IsDefault == true)
                    .ToListAsync();
                existing.ForEach(a => a.IsDefault = false);
            }

            var address = new Address
            {
                AddressId = Guid.NewGuid(),
                CustomerId = dto.CustomerId,
                Province = dto.Province,
                District = dto.District,
                Ward = dto.Ward,
                AddressDetails = dto.AddressDetails,
                IsDefault = dto.IsDefault ?? false,
                IsActive = true
            };

            _db.Addresses.Add(address);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, addressId = address.AddressId });
        }

        // PUT: address-service/Addresses/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AddressCreateRequest dto)
        {
            var address = await _db.Addresses.FindAsync(id);
            if (address == null) return NotFound();

            if (dto.IsDefault == true)
            {
                var existing = await _db.Addresses
                    .Where(a => a.CustomerId == address.CustomerId && a.IsDefault == true && a.AddressId != id)
                    .ToListAsync();
                existing.ForEach(a => a.IsDefault = false);
            }

            address.Province = dto.Province;
            address.District = dto.District;
            address.Ward = dto.Ward;
            address.AddressDetails = dto.AddressDetails;
            address.IsDefault = dto.IsDefault ?? false;

            await _db.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // DELETE: address-service/Addresses/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var address = await _db.Addresses.FindAsync(id);
            if (address == null) return NotFound();

            address.IsActive = false;
            await _db.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }

    public class AddressCreateRequest
    {
        public Guid CustomerId { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? AddressDetails { get; set; }
        public bool? IsDefault { get; set; }
    }
}
