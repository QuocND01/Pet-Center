using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;
using PetCenterAPI.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetCenterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/
        [HttpGet]
        public async Task<IActionResult> Get(ODataQueryOptions<ReadProductDTOForCustomer> queryOptions)
        {
            var result = await _productService.GetAllProductAsync(queryOptions);
            return Ok(result);
        }


        [HttpGet("admin")]
        public async Task<ActionResult<PagedResult<ReadProductDTO>>> GetAllProductAdminAsync(
    [FromQuery] ProductSpecification spec)
        {
            var result = await _productService.GetAllProductAdminAsync(spec);
            return Ok(result);
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadProductDTO>> DetailsProductAsync(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductAsync(
            Guid id,
            [FromForm] UpdateProductDTO product)
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = string.Join(", ", errors) });
            }

            try
            {
                await _productService.UpdateProductAsync(id, product);

                return Ok(new { success = true, message = "Product updated successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.ToString()
                });
            }
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> PostProductAsync([FromForm] CreateProductDTO product)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    success = false,
                    message = string.Join(", ", errors)
                });
            }

            try
            {
                await _productService.AddProductAsync(product);

                return Ok(new
                {
                    success = true,
                    message = "Product created successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // DELETE: api/Products/5
        // [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(
      Guid id,
      [FromBody] Status status)
        {
            Console.WriteLine(status);
            try
            {
                await _productService.ChangeProductStatusAsync(id, status);

                return Ok(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== ERROR =====");
                Console.WriteLine(ex.ToString());

                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    stack = ex.StackTrace
                });
            }
        }

        [HttpGet("select")]
        public async Task<IActionResult> GetProductSelectAsync()
        {
            var result = await _productService.GetProductSelectListAsync();
            return Ok(result);
        }

        [HttpGet("selecttoview")]
        public async Task<IActionResult> GetProductSelectToViewAsync()
        {
            var result = await _productService.GetProductSelectListToViewAsync();
            return Ok(result);
        }


        // Lấy sản phẩm mới
        [HttpGet("new-products")]
        public async Task<IActionResult> GetNewProductsAsync()
        {
            var products = await _productService.GetNewProductsAsync();
            return Ok(products);
        }

        // Lấy sản phẩm hot (bán chạy)
        [HttpGet("hot-products")]
        public async Task<IActionResult> GetHotProductsAsync()
        {
            var products = await _productService.GetHotProductsAsync();
            return Ok(products);
        }


        //Code Hồ mới thêm
        [HttpGet("internal/{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInternal(Guid id)
        {
            var result = await _productService.GetInternalAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
        [HttpPost("snapshot")]
        public async Task<IActionResult> GetSnapshots(
            [FromBody] ProductSnapshotRequestDto dto)
        {
            if (dto.ProductIds == null || !dto.ProductIds.Any())
            {
                return BadRequest("ProductIds is required");
            }

            var result = await _productService
                .GetProductSnapshotsAsync(dto.ProductIds);

            return Ok(result);
        }
    }
}
