using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using PetCenterClient.DTOs;
using PetCenterClient.Services;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        // GET: BrandController
        public async Task<ActionResult> Index(string? search, int page = 1)
        {
            var result = await _categoryService.GetAllCategoryAsync(search, page);
            return View("~/Views/CustomerViews/Home/HomePage.cshtml", result);
        }

        public async Task<ActionResult> Indexadmin(string? search, int page = 1)
        {
            var result = await _categoryService.GetAllCategoryAsync(search, page);
            return View("~/Views/AdminViews/Category/Index.cshtml", result);
        }

        // GET: CategoryController/Details/5
        public async Task<ActionResult> Details(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readCategory = await _categoryService.GetCategoryByIdAsync(id);
            if (readCategory == null)
            {
                return NotFound();
            }
            return PartialView("~/Views/CustomerView/Category/_Details.cshtml", readCategory);
        }



        public async Task<ActionResult> DetailsAsyncAdmin(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readCategory = await _categoryService.GetCategoryByIdAsync(id);
            if (readCategory == null)
            {
                return NotFound();
            }
            return PartialView("~/Views/AdminViews/Category/_Details.cshtml", readCategory);
        }

        // GET: CategoryController/Create
        public ActionResult Create()
        {
            return PartialView("~/Views/AdminViews/Category/_Create.cshtml");
        }

        // POST: CategoryController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateCategoryDTOs model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return PartialView("~/Views/AdminViews/Category/_Create.cshtml", model);
            }
            try
            {
                await _categoryService.AddCategoryAsync(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Json(new { success = true });
        }

        // GET: CategoryController/Edit/5
        public async Task<ActionResult> Edit(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var updateCategory = await _categoryService.GetCategoryByIdAsync(id);
            if (updateCategory == null)
            {
                return NotFound();
            }
            return PartialView("~/Views/AdminViews/Category/_Edit.cshtml", updateCategory);
        }

        // POST: CategoryController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateCategoryDTOs model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid data"
                });
            }

            await _categoryService.UpdateCategoryAsync(id, model);

            return Json(new { success = true });
        }

        // GET: CategoryController/Delete/5
        public async Task<ActionResult> Delete(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _categoryService.GetCategoryByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return PartialView("~/Views/AdminViews/Category/_Delete.cshtml", model);
        }

        // POST: CategoryController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirm(Guid id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

    }
}
