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
        [EnableQuery]
        public IQueryable<ReadProductDTO> Get()
        {
            return _productService.GetAllProduct();
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
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(Guid id,[FromForm] UpdateProductDTO product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _productService.UpdateProductAsync(id, product);
                return Ok(product);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> PostProduct([FromForm] CreateProductDTO product)
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
           .SelectMany(v => v.Errors)
           .Select(e => e.ErrorMessage)
           .ToList();

                return BadRequest(errors);
            }

                try
            {
                await _productService.AddProductAsync(product);
                return Ok(product);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message); // 409
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/Products/5
        [Authorize(Roles = "Admin")]
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
        [Authorize]
        [HttpPost("increase-stock-bulk")]
        public async Task<IActionResult> IncreaseStockBulk([FromBody] List<IncreaseStockItemDto> items)
        {
            await _productService.IncreaseStockBulk(items);
            return Ok();
        }
        [Authorize]
        [HttpPut("decrease-stock/{id}")]
        public async Task<IActionResult> DecreaseStock(Guid id, [FromBody] int quantity)
        {
            var success = await _productService.DecreaseStockAsync(id, quantity);

            if (!success)
            {
                return BadRequest(new { message = "Sản phẩm không tồn tại hoặc số lượng tồn kho không đủ để xử lý." });
            }

            return Ok(true);
        }
        [Authorize]
        [HttpPut("increase-stock/{id}")]
        public async Task<IActionResult> IncreaseStock(Guid id, [FromBody] int quantity)
        {
            var success = await _productService.IncreaseStockAsync(id, quantity);

            if (!success)
            {
                return BadRequest(new { message = "Sản phẩm không tồn tại để cộng lại kho." });
            }

            return Ok(true);
        }
    }
}
