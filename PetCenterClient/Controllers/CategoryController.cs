using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using PetCenterClient.Common;
using PetCenterClient.Services;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.Category;

namespace PetCenterClient.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryAPIClient _categoryService;

        public CategoryController(ICategoryAPIClient categoryService)
        {
            _categoryService = categoryService;
        }
        // GET: BrandController
        public async Task<IActionResult> IndexAsync()
        {
            var result = await _categoryService.GetAllCategoryAsync();

            return View("~/Views/CustomerViews/Home/ProductPage.cshtml", result);
        }

        public async Task<IActionResult> IndexAdminAsync(
            string? search, Status? status, int page = 1)
        {
            var result = await _categoryService.GetAllCategoryAdminAsync(search, status, page);

            ViewBag.CurrentPage = result.CurrentPage;
            ViewBag.TotalCategories = result.TotalCount;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.Search = search;
            ViewBag.Status = status;

            return View("~/Views/AdminViews/Category/Index.cshtml", result.Data);
        }

        // GET: CategoryController/Details/5
        public async Task<IActionResult> DetailsAsync(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readCategory = await _categoryService.DetailsCategoryAsync(id);
            if (readCategory == null)
            {
                return NotFound();
            }
            return PartialView("~/Views/CustomerView/Category/_Details.cshtml", readCategory);
        }



        public async Task<IActionResult> DetailsAdminAsync(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readCategory = await _categoryService.DetailsCategoryAsync(id);
            if (readCategory == null)
            {
                return NotFound();
            }
            return PartialView("~/Views/AdminViews/Category/_Details.cshtml", readCategory);
        }

        // GET: CategoryController/Create
        public ActionResult CreateAsync()
        {
            return PartialView("~/Views/AdminViews/Category/_Create.cshtml", new CreateCategoryViewModel());

        }

        // POST: CategoryController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(CreateCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/AdminViews/Category/_Create.cshtml", model);
            }

            try
            {
                await _categoryService.AddCategoryAsync(model);

                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);

                return PartialView("~/Views/AdminViews/Category/_Create.cshtml", model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred.");

                return PartialView("~/Views/AdminViews/Category/_Create.cshtml", model);
            }
        }

        // GET: CategoryController/Edit/5
        public async Task<IActionResult> EditAsync(Guid id)
        {
            var category = await _categoryService.DetailsCategoryAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var model = new UpdateCategoryViewModel
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription,
                ExistingCategoryLogo = category.CategoryLogo,
                Status = category.Status,

                Attributes = category.Attributes?
                .Select(a => new UpdateCategoryAttributeViewModel
                {
                    CategoryAttributeId = a.CategoryAttributeId,
                    AttributeName = a.AttributeName
                }).ToList()
            };

            return PartialView("~/Views/AdminViews/Category/_Edit.cshtml", model);
        }

        // POST: CategoryController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(Guid id, UpdateCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/AdminViews/Category/_Edit.cshtml", model);
            }

            try
            {
                await _categoryService.UpdateCategoryAsync(id, model);

                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);

                return PartialView("~/Views/AdminViews/Category/_Edit.cshtml", model);
            }
        }



        public async Task<IActionResult> ChangeStatusAsync(
     Guid id,
     Status status)
        {
            var model = await _categoryService.DetailsCategoryAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            ViewBag.Status = status;

            return PartialView(
                "~/Views/AdminViews/Category/_Delete.cshtml",
                model);
        }

        [HttpPost]
        [ActionName("ChangeStatusConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatusConfirmAsync(
            Guid id,
            Status status)
        {
            Console.WriteLine($"Category ID = {id}");
            Console.WriteLine($"Status = {status}");
            try
            {
                await _categoryService.ChangeCategoryStatusAsync(
                    id,
                    status);

                return Json(new
                {
                    success = true
                });
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
