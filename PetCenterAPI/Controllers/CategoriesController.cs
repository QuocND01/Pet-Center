using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;
using PetCenterAPI.Service;
using PetCenterAPI.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PetCenterAPI.DTOs.Requests.Category.CategoryRequestDTO;
using static PetCenterAPI.DTOs.Responses.Category.CategoryResponseDTO;
namespace PetCenterAPI.Controllers
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
        public IQueryable<ReadCategoryDTOForCustomer> Get()
        {
            return _categoryService.GetAllCategory();
        }

        [HttpGet("admin")]
        public async Task<ActionResult<PagedResult<ReadCategoryDTO>>> GetAllCategoryAdminAsync(
    [FromQuery] CategorySpecification spec)
        {
            var result = await _categoryService.GetAllCategoryAdminAsync(spec);
            return Ok(result);
        }

        [HttpGet("{id}/attributes")]
        public async Task<IActionResult> GetAttributesAsync(Guid id)
        {
            var attributes = await _categoryService.GetAllCategoryAttributeByCategoryIDAsync(id);
            return Ok(attributes);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ReadCategoryDTO>> DetailsCategoryAsync(Guid id)
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
        public async Task<IActionResult> PutCategoryAsync(
      Guid id,
      [FromForm] UpdateCategoryDTO updateCategoryDTOs)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    status = false,
                    message = string.Join(", ", errors)
                });
            }

            try
            {
                await _categoryService.UpdateCategoryAsync(id, updateCategoryDTOs);

                return Ok(new
                {
                    status = true,
                    message = "Category updated successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    status = false,
                    message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    status = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }


        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> PostCategoryAsync([FromForm] CreateCategoryDTO categoryDTOs)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    status = false,
                    message = string.Join(", ", errors)
                });
            }

            try
            {
                await _categoryService.AddCategoryAsync(categoryDTOs);

                return Ok(new
                {
                    status = true,
                    message = "Category created successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    status = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }


        //[Authorize]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatusAsync(
     Guid id,
     [FromBody] Status status)
        {
            await _categoryService.ChangeCategoryStatusAsync(id, status);

            return Ok(new
            {
                status = true
            });
        }

    }
}
