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
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService context)
        {
            _categoryService = context;
        }

        // GET: api/Categories
        [HttpGet]
        [EnableQuery(PageSize = 10)]
        public IQueryable<ReadCategoryDTOs> Get()
        {
            return _categoryService.GetAllCategory();
        }

        [HttpGet("{id}/attributes")]
        public async Task<IActionResult> GetAttributes(Guid id)
        {
            var attributes = await _categoryService.GetAllCategoryAttributeByCategoryIDAsync(id);
            return Ok(attributes);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ReadCategoryDTOs>> GetCategoryId(Guid id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }


        // [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(
      Guid id,
      [FromForm] UpdateCategoryDTOs updateCategoryDTOs)
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
                await _categoryService.UpdateCategoryAsync(id, updateCategoryDTOs);

                return Ok(new
                {
                    success = true,
                    message = "Category updated successfully"
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
        public async Task<IActionResult> PostCategory([FromForm] CreateCategoryDTOs categoryDTOs)
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
                await _categoryService.AddCategoryAsync(categoryDTOs);

                return Ok(new
                {
                    success = true,
                    message = "Category created successfully"
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
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            await _categoryService.DeleteCategoryAsync(id);

            return NoContent();
        }

    }
}
