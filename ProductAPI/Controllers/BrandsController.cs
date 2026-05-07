using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using ProductAPI.DTOs;
using ProductAPI.Models;
using ProductAPI.Service;
using ProductAPI.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandsController(IBrandService context)
        {
            _brandService = context;
        }

        [HttpGet]
        [EnableQuery(PageSize = 10)]
        public IQueryable<ReadBrandDTOs> Get()
        {
            return _brandService.GetAllBrand();
        }

      
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadBrandDTOs>> DetailsBrandAsync(Guid id)
        {
            var brand = await _brandService.GetBrandByIdAsync(id);

            if (brand == null)
            {
                return NotFound();
            }

            return brand;
        }

        //[Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBrandAsync(
            Guid id,
            [FromForm] UpdateBrandDTOs updateBrand)
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
                    message = errors
                });
            }

            try
            {
                await _brandService.UpdateBrandAsync(id, updateBrand);

                return Ok(new
                {
                    success = true,
                    message = "Brand updated successfully"
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
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
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

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> PostBrandAsync([FromForm] CreateBrandDTOs createBrand)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { success = false, message = errors });
            }

            try
            {
                await _brandService.AddBrandAsync(createBrand);

                return Ok(new
                {
                    success = true,
                    message = "Brand created successfully"
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

        //[Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrandAsync(Guid id)
        {
            var brand = await _brandService.GetBrandByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            await _brandService.DeleteBrandAsync(id);

            return Ok();
        }
    }
}
