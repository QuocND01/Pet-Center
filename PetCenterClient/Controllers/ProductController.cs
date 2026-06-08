using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetCenterClient.Common;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductServiceClient _productService;
        private readonly IBrandServiceClient _brandService;
        private readonly ICategoryServiceClient _categoryService;
        private readonly IFeedbackAPIClient _feedbackService;
        private readonly ICustomerAPIClient _customerService;

        public ProductsController(IProductServiceClient productService, IBrandServiceClient brandService, ICategoryServiceClient categoryService, IFeedbackAPIClient feedbackService, ICustomerAPIClient customerService)
            
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
               search, isActive, minPrice, maxPrice, fromDate, toDate, sortBy, categoryid, brandid, sortOrder, page);
            int totalItems = result?.Count ?? 0;
            var totalPages = (int)Math.Ceiling((double)totalItems / pagesize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            var hotProducts = await _productService.GetHotProductsAsync();
            var newProducts = await _productService.GetNewProductsAsync();
            ViewBag.HotProducts = hotProducts;
            ViewBag.NewProducts = newProducts;
            ViewBag.PageSize = pagesize;
            ViewBag.Brands = await _brandService.GetAllBrandAsync("", 1);
            ViewBag.Categories = await _categoryService.GetAllCategoryAsync("", 1);
            return View("~/Views/CustomerViews/Home/HomePage.cshtml", result);
        }



        public async Task<IActionResult> IndexAdminAsync(
     string? search,
     bool? isActive,
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
                search, isActive, minPrice, maxPrice, categoryId, brandId,
                sortBy, fromDate, toDate, sortOrder, page);

            ViewBag.CurrentPage = result.CurrentPage;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.Search = search;
            ViewBag.IsActive = isActive;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.CategoryId = categoryId;
            ViewBag.BrandId = brandId;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

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

            var customerIds = feedbacks
        .Select(f => f.CustomerId)
        .Distinct()
        .ToList();

            var customerNameTasks = customerIds
                .ToDictionary(
                    cid => cid,
                    cid => _customerService.GetDisplayNameAsync(cid)
                );

            await Task.WhenAll(customerNameTasks.Values);

            var customerNames = customerNameTasks.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Result
            );

            // Thống kê rating
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
            ViewBag.CustomerNames = customerNames;
            ViewBag.TotalCount = totalCount;
            ViewBag.AvgRating = avgRating;
            ViewBag.RatingCounts = ratingCounts;

            return View("~/Views/CustomerViews/Product/Details.cshtml", readProdutDTOs);
        }

        // GET: ReadProdutDTOs/Create
        public async Task<IActionResult> CreateAsync()
        {
            var brands = await _brandService.GetAllBrandAsync("", 1) ?? new OdataResponse<ReadBrandDTOForCustomer>();
            var categories = await _categoryService.GetAllCategoryAsync("", 1) ?? new OdataResponse<ReadCategoryDTOForCustomer>();
            ViewBag.Brands = new SelectList(brands.Values, "BrandId", "BrandName");
            ViewBag.Categories = new SelectList(categories.Values, "CategoryId", "CategoryName");

            return PartialView("~/Views/AdminViews/Product/_Create.cshtml");
        }

        // POST: ReadProdutDTOs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(CreateProductDTO model)
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
                    .Select(a => new CreateProductAttributeDTO
                    {
                        CategoryAttributeId = a.CategoryAttributeId,
                        AttributeName = a.AttributeName,
                        AttributeValue = model.Attributes?
                            .FirstOrDefault(x => x.CategoryAttributeId == a.CategoryAttributeId)?
                            .AttributeValue
                    }).ToList();

                var brands = await _brandService.GetAllBrandAsync("", 1);
                var categories = await _categoryService.GetAllCategoryAsync("", 1);

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
                var brands = await _brandService.GetAllBrandAsync("", 1);
                var categories = await _categoryService.GetAllCategoryAsync("", 1);

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

            var model = new UpdateProductDTO
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
                    .Select(a => new UpdateProductAttributeDTO
                    {
                        CategoryAttributeId = a.CategoryAttributeId,
                        AttributeName = a.AttributeName,
                        AttributeValue = a.AttributeValue
                    }).ToList()
            };

            var brands = await _brandService.GetAllBrandAsync("", 1);
            var categories = await _categoryService.GetAllCategoryAsync("", 1);

            ViewBag.Brands = new SelectList(brands.Values, "BrandId", "BrandName");
            ViewBag.Categories = new SelectList(categories.Values, "CategoryId", "CategoryName");

            return PartialView("~/Views/AdminViews/Product/_Edit.cshtml", model);
        }

        // POST: ReadProdutDTOs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(Guid ProductId, UpdateProductDTO model)
        {
            if (!ModelState.IsValid)
            {
                var brands = await _brandService.GetAllBrandAsync("", 1);
                var categories = await _categoryService.GetAllCategoryAsync("", 1);
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

                var brands = await _brandService.GetAllBrandAsync("", 1);
                var categories = await _categoryService.GetAllCategoryAsync("", 1);

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
