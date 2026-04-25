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

        // GET: api/Brands
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Brand>>> GetBrands()
        //{
        //    var brands = await _brandService.GetAllBrandAsync();
        //    return Ok(brands);
        //}

        [HttpGet]
        [EnableQuery(PageSize = 10)]
        public IQueryable<ReadBrandDTOs> Get()
        {
            return _brandService.GetAllBrand();
        }

      
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadBrandDTOs>> GetBrandByID(Guid id)
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
        public async Task<IActionResult> PutBrand(Guid id, UpdateBrandDTOs upadteBrand)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _brandService.UpdateBrandAsync(id, upadteBrand);
                return Ok(upadteBrand);
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

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> PostBrand( CreateBrandDTOs createBrand)
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
                await _brandService.AddBrandAsync(createBrand);
                return Ok();
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

        //[Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand(Guid id)
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
