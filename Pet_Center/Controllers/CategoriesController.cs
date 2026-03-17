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



        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(Guid id, UpdateCategoryDTOs updateCategoryDTOs)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _categoryService.UpdateCategoryAsync(id, updateCategoryDTOs);
                return Ok(updateCategoryDTOs);
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

   

        [HttpPost]
        public async Task<IActionResult> PostCategory( CreateCategoryDTOs categoryDTOs)
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
                await _categoryService.AddCategoryAsync(categoryDTOs);
                return Ok(categoryDTOs);
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
