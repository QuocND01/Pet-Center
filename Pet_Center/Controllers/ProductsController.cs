using Humanizer;
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

        // GET: api/Products
        [HttpGet]
        [EnableQuery(PageSize = 10)]
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
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(Guid id,[FromForm] UpdateProductDTO product)
        {
            await _productService.UpdateProductAsync(id, product);
            return Ok(product);
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostProduct([FromForm] CreateProductDTO product)
        {
            if (product == null)
                return BadRequest("Product is null");
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await _productService.DeleteProductAsync(id);

            return NoContent();
        }

        [HttpGet("select")]
        public async Task<IActionResult> GetProductSelect()
        {
            var result = await _productService.GetProductSelectListAsync();
            return Ok(result);
        }
    }
}
