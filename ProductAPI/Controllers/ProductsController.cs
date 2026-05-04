using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using ProductAPI.DTOs;
using ProductAPI.Models;
using ProductAPI.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductAPI.Controllers
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
        public async Task<IActionResult> Get(ODataQueryOptions<ReadProductDTO> queryOptions)
        {
            var result = await _productService.GetAllProductAsync(queryOptions);
            return Ok(result);
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadProductDTO>> GetProduct(Guid id)
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
        public async Task<IActionResult> PutProduct(
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
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message }); // 409
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message }); // 404
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> PostProduct([FromForm] CreateProductDTO product)
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            await _productService.DeleteProductAsync(id);

            return NoContent();
        }

        [HttpGet("select")]
        public async Task<IActionResult> GetProductSelect()
        {
            var result = await _productService.GetProductSelectListAsync();
            return Ok(result);
        }

        [HttpGet("selecttoview")]
        public async Task<IActionResult> GetProductSelectToView()
        {
            var result = await _productService.GetProductSelectListToViewAsync();
            return Ok(result);
        }


        // Lấy sản phẩm mới
        [HttpGet("new-products")]
        public async Task<IActionResult> GetNewProducts()
        {
            var products = await _productService.GetNewProducts();
            return Ok(products);
        }

        // Lấy sản phẩm hot (bán chạy)
        [HttpGet("hot-products")]
        public async Task<IActionResult> GetHotProducts()
        {
            var products = await _productService.GetHotProducts();
            return Ok(products);
        }
        //[Authorize]
        //[HttpPost("increase-stock-bulk")]
        //public async Task<IActionResult> IncreaseStockBulk([FromBody] List<IncreaseStockItemDto> items)
        //{
        //    await _productService.IncreaseStockBulk(items);
        //    return Ok();
        //}
        //[Authorize]
        //[HttpPut("decrease-stock/{id}")]
        //public async Task<IActionResult> DecreaseStock(Guid id, [FromBody] int quantity)
        //{
        //    var success = await _productService.DecreaseStockAsync(id, quantity);

        //    if (!success)
        //    {
        //        return BadRequest(new { message = "Sản phẩm không tồn tại hoặc số lượng tồn kho không đủ để xử lý." });
        //    }

        //    return Ok(true);
        //}
        //[Authorize]
        //[HttpPut("increase-stock/{id}")]
        //public async Task<IActionResult> IncreaseStock(Guid id, [FromBody] int quantity)
        //{
        //    var success = await _productService.IncreaseStockAsync(id, quantity);

        //    if (!success)
        //    {
        //        return BadRequest(new { message = "Sản phẩm không tồn tại để cộng lại kho." });
        //    }

        //    return Ok(true);
        //}

        //// GET: api/Products/{id}/include-deleted
        //[HttpGet("{id}/include-deleted")]
        //public async Task<ActionResult<ReadProductDTO>> GetProductIncludeDeleted(Guid id)
        //{
        //    var product = await _productService.GetProductByIdIncludeDeletedAsync(id);

        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    return product;
        //}

        //Code Hồ mới thêm
        [HttpGet("internal/{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInternal(Guid id)
        {
            var result = await _productService.GetInternalAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
