using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PetCenterClient.Common;
using PetCenterClient.Services;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class InventoryController : Controller
    {
        private readonly InventoryApiService _inventoryApiService;
        private readonly IBrandAPIClient _brandApi;
        private readonly ICategoryAPIClient _categoryApi;

        public InventoryController(
            InventoryApiService inventoryApi,
            IBrandAPIClient brandApi,
            ICategoryAPIClient categoryApi)
        {
            _inventoryApiService = inventoryApi;
            _brandApi = brandApi;
            _categoryApi = categoryApi;
        }
        public async Task<IActionResult> Index(
                string? keyword,
                Guid? categoryId,
                Guid? brandId,
                bool? lowStock,
                bool? outOfStock,
                int page = 1,
                int pageSize = 10)
        {
            var inventory = await _inventoryApiService.GetPagedAsync(
                keyword,
                categoryId,
                brandId,
                lowStock,
                outOfStock,
                page,
                pageSize);

            var brands =
                await _brandApi.GetAllBrandAdminAsync(
                    null,
                    Status.Active,
                    1,
                    1000);

            var categories =
                await _categoryApi.GetAllCategoryAdminAsync(
                    null,
                    Status.Active,
                    1,
                    1000);

            ViewBag.BrandList = brands.Data
                .Select(x => new SelectListItem
                {
                    Value = x.BrandId.ToString(),
                    Text = x.BrandName
                })
                .ToList();

            ViewBag.CategoryList = categories.Data
                .Select(x => new SelectListItem
                {
                    Value = x.CategoryId.ToString(),
                    Text = x.CategoryName
                })
                .ToList();

            ViewBag.Keyword = keyword;
            ViewBag.BrandId = brandId;
            ViewBag.CategoryId = categoryId;
            ViewBag.LowStock = lowStock;
            ViewBag.OutOfStock = outOfStock;

            return View("~/Views/AdminViews/Inventory/Index.cshtml",inventory);
        }
        public async Task<IActionResult> Details(Guid id)
        {
            var inventory = await _inventoryApiService.GetByIdAsync(id);

            if (inventory == null)
                return NotFound();

            return View("~/Views/AdminViews/Inventory/Detail.cshtml", inventory);
        }
    }

}
