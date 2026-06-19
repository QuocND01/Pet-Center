using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetCenterClient.Common;
using PetCenterClient.DTOs;
using PetCenterClient.Services;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.Brand;
using PetCenterClient.ViewModels.Category;
using PetCenterClient.ViewModels.Common;
using PetCenterClient.ViewModels.Product;

namespace PetCenterClient.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductAPIClient _productService;
        private readonly IBrandAPIClient _brandService;
        private readonly ICategoryAPIClient _categoryService;
        private readonly IFeedbackApiService _feedbackService;
        private readonly ICustomerApiService _customerService;

        public ProductsController(IProductAPIClient productService, IBrandAPIClient brandService, ICategoryAPIClient categoryService, IFeedbackApiService feedbackService, ICustomerApiService customerService)

        {
            _productService = productService;
            _brandService = brandService;
            _categoryService = categoryService;
            _feedbackService = feedbackService;
            _customerService = customerService;
        }

        // GET: ReadProdutDTOs
        public async Task<IActionResult> IndexAsync(
        string? search,
        bool? isActive,
        decimal? minPrice,
        decimal? maxPrice,
        DateTime? fromDate,
        DateTime? toDate,
        string? sortBy,
        Guid? categoryid,
        Guid? brandid,
        string sortOrder = "asc",
        int page = 1)
        {
            int pagesize = 24;

            var result = await _productService.GetAllProductAsync(
                search,
                minPrice,
                maxPrice,
                fromDate,
                toDate,
                sortBy,
                categoryid,
                brandid,
                sortOrder,
                page);

            int totalItems = result?.Count ?? 0;
            var totalPages = (int)Math.Ceiling((double)totalItems / pagesize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pagesize;

            ViewBag.Search = search;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.CategoryId = categoryid;
            ViewBag.BrandId = brandid;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            // Hot/New
            ViewBag.HotProducts = await _productService.GetHotProductsAsync();
            ViewBag.NewProducts = await _productService.GetNewProductsAsync();

            // Categories
            var categoriesResponse = await _categoryService.GetAllCategoryAsync();
            ViewBag.Categories = categoriesResponse.Values;

            // Brands
            var brandsResponse = await _brandService.GetAllBrandAsync();
            ViewBag.Brands = brandsResponse.Values;

            // Category đang được chọn
            if (categoryid.HasValue)
            {
                ViewBag.SelectedCategory =
                    categoriesResponse.Values
                    .FirstOrDefault(x => x.CategoryId == categoryid.Value);
            }

            // Brand đang được chọn
            if (brandid.HasValue)
            {
                ViewBag.SelectedBrand =
                    brandsResponse.Values
                    .FirstOrDefault(x => x.BrandId == brandid.Value);
            }

            return View(
                "~/Views/CustomerViews/Home/ProductPage.cshtml",
                result);
        }


        public async Task<IActionResult> IndexAdminAsync(
     string? search,
    Status? status,
     decimal? minPrice,
     decimal? maxPrice,
     Guid? categoryId,
     Guid? brandId,
     string? sortBy,
     DateTime? fromDate,
     DateTime? toDate,
     string sortOrder = "asc",
     int page = 1)
        {
            var result = await _productService.GetAllProductAdminAsync(
                search, status, minPrice, maxPrice, categoryId, brandId,
                sortBy, fromDate, toDate, sortOrder, page);

            ViewBag.CurrentPage = result.CurrentPage;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalProducts = result.TotalCount;
            ViewBag.Search = search;
            ViewBag.IsActive = status;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.CategoryId = categoryId;
            ViewBag.BrandId = brandId;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            var categories = await _categoryService.GetAllCategoryAdminAsync(null,null);
            ViewBag.Categories = categories.Data;

            var brands = await _brandService.GetAllBrandAdminAsync(null,null);
            ViewBag.Brands = brands.Data;

            return View("~/Views/AdminViews/Product/Index.cshtml", result.Data);
        }


        // GET: ReadProdutDTOs/Details/5
        public async Task<IActionResult> DetailsAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readProdutDTOs = await _productService.DetailsProductAsync(id);
            if (readProdutDTOs == null)
            {
                return NotFound();
            }
            return PartialView("~/Views/AdminViews/Product/_Details.cshtml", readProdutDTOs);
        }

        public async Task<IActionResult> DetailsForcustomerAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readProdutDTOs = await _productService.DetailsProductAsync(id);
            if (readProdutDTOs == null)
            {
                return NotFound();
            }

            // Load feedbacks của sản phẩm này
            var feedbacks = await _feedbackService.GetFeedbacksByProductIdAsync(id.Value);

            // Rating statistics
            var totalCount = feedbacks.Count;
            var avgRating = totalCount > 0
                ? Math.Round(feedbacks.Average(f => f.Rating ?? 0), 1)
                : 0.0;

            var ratingCounts = Enumerable.Range(1, 5)
                .ToDictionary(
                    star => star,
                    star => feedbacks.Count(f => f.Rating == star)
                );

            ViewBag.Feedbacks = feedbacks;
            ViewBag.TotalCount = totalCount;
            ViewBag.AvgRating = avgRating;
            ViewBag.RatingCounts = ratingCounts;

            return View("~/Views/CustomerViews/Product/Details.cshtml", readProdutDTOs);
        }

        // GET: ReadProdutDTOs/Create
        public async Task<IActionResult> CreateAsync()
        {
            var brands = await _brandService.GetAllBrandAsync() ?? new OdataResponse<ReadBrandViewModelForCustomer>();
            var categories = await _categoryService.GetAllCategoryAsync() ?? new OdataResponse<ReadCategoryViewModelForCustomer>();
            ViewBag.Brands = new SelectList(brands.Values, "BrandId", "BrandName");
            ViewBag.Categories = new SelectList(categories.Values, "CategoryId", "CategoryName");

            return PartialView("~/Views/AdminViews/Product/_Create.cshtml");
        }

        // POST: ReadProdutDTOs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(CreateProductViewModel model)
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
       .SelectMany(v => v.Errors)
       .Select(e => e.ErrorMessage)
       .ToList();

                Console.WriteLine(string.Join(",", errors));
                var category = await _categoryService.DetailsCategoryAsync(model.CategoryId);

                model.Attributes = category.Attributes
                    .Select(a => new CreateProductAttributeViewModel
                    {
                        CategoryAttributeId = a.CategoryAttributeId,
                        AttributeName = a.AttributeName,
                        AttributeValue = model.Attributes?
                            .FirstOrDefault(x => x.CategoryAttributeId == a.CategoryAttributeId)?
                            .AttributeValue
                    }).ToList();

                var brands = await _brandService.GetAllBrandAsync();
                var categories = await _categoryService.GetAllCategoryAsync();

                ViewBag.Brands = new SelectList(brands.Values, "BrandId", "BrandName");
                ViewBag.Categories = new SelectList(categories.Values, "CategoryId", "CategoryName");
                return PartialView("~/Views/AdminViews/Product/_Create.cshtml", model);
            }

            try
            {
                await _productService.AddProductAsync(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var brands = await _brandService.GetAllBrandAsync();
                var categories = await _categoryService.GetAllCategoryAsync();

                ViewBag.Brands = new SelectList(brands.Values, "BrandId", "BrandName");
                ViewBag.Categories = new SelectList(categories.Values, "CategoryId", "CategoryName");
                return PartialView("~/Views/AdminViews/Product/_Create.cshtml", model);
            }
        }

        // GET: ReadProdutDTOs/Edit/5
        public async Task<IActionResult> EditAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productService.DetailsProductAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var model = new UpdateProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ProductPrice = product.ProductPrice,
                ProductDescription = product.ProductDescription,
                BrandId = product.BrandId,
                BrandName = product.BrandName,
                CategoryId = product.CategoryId,
                CategoryName = product.CategoryName,
                Status = product.Status,
                ExistingImages = product.Images,
                Attributes = product.Attributes?
                    .Select(a => new UpdateProductAttributeViewModel
                    {
                        CategoryAttributeId = a.CategoryAttributeId,
                        AttributeName = a.AttributeName,
                        AttributeValue = a.AttributeValue
                    }).ToList()
            };

            var brands = await _brandService.GetAllBrandAsync();
            var categories = await _categoryService.GetAllCategoryAsync();

            ViewBag.Brands = new SelectList(brands.Values, "BrandId", "BrandName");
            ViewBag.Categories = new SelectList(categories.Values, "CategoryId", "CategoryName");

            return PartialView("~/Views/AdminViews/Product/_Edit.cshtml", model);
        }

        // POST: ReadProdutDTOs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(Guid ProductId, UpdateProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var brands = await _brandService.GetAllBrandAsync();
                var categories = await _categoryService.GetAllCategoryAsync();
                ViewBag.Brands = new SelectList(brands.Values, "BrandId", "BrandName");
                ViewBag.Categories = new SelectList(categories.Values, "CategoryId", "CategoryName");
                foreach (var item in ModelState)
                {
                    foreach (var error in item.Value.Errors)
                    {
                        Console.WriteLine($"KEY: {item.Key}");
                        Console.WriteLine($"ERROR: {error.ErrorMessage}");
                    }
                }
                return PartialView("~/Views/AdminViews/Product/_Edit.cshtml", model);
            }

            try
            {
                await _productService.UpdateProductAsync(ProductId, model);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                var brands = await _brandService.GetAllBrandAsync();
                var categories = await _categoryService.GetAllCategoryAsync();

                ViewBag.Brands = new SelectList(brands.Values, "BrandId", "BrandName");
                ViewBag.Categories = new SelectList(categories.Values, "CategoryId", "CategoryName");
                return PartialView("~/Views/AdminViews/Product/_Edit.cshtml", model);
            }
        }

        // GET: ReadProdutDTOs/Delete/5
        public async Task<IActionResult> ChangeStatusAsync(
      Guid? id,
      Status status)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _productService.DetailsProductAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            ViewBag.Status = status;

            return PartialView(
                "~/Views/AdminViews/Product/_Delete.cshtml",
                model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatusConfirmedAsync(
            Guid id,
            Status status)
        {
            await _productService.ChangeProductStatusAsync(
                id,
                status);

            return Json(new
            {
                success = true
            });
        }

    }

}
