using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly IBrandService _brandService;
        private readonly ICategoryService _categoryService;
        private readonly IFeedbackService _feedbackService;

        public ProductsController(IProductService productService, IBrandService brandService, ICategoryService categoryService, IFeedbackService feedbackService)
        {
            _productService = productService;
            _brandService = brandService;
            _categoryService = categoryService;
            _feedbackService = feedbackService;
        }

        // GET: ReadProdutDTOs
        public async Task<IActionResult> Index(
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



        public async Task<IActionResult> Indexadmin(
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
            var totalPages = (int)Math.Ceiling((decimal)(totalItems / (decimal)pagesize));
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pagesize;
            return View("~/Views/AdminViews/Product/Index.cshtml", result);
        }


        // GET: ReadProdutDTOs/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readProdutDTOs = await _productService.GetProductByIdAsync(id);
            if (readProdutDTOs == null)
            {
                return NotFound();
            }
            return PartialView("~/Views/AdminViews/Product/_Details.cshtml", readProdutDTOs);
        }

        public async Task<IActionResult> DetailsForcustomer(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readProdutDTOs = await _productService.GetProductByIdAsync(id);
            if (readProdutDTOs == null)
            {
                return NotFound();
            }

            //var feedbacks = await _feedbackService.GetByProductAsync(id.Value);
            //ViewBag.Feedbacks = feedbacks;

            return View("~/Views/CustomerViews/Product/Details.cshtml", readProdutDTOs);
        }

        // GET: ReadProdutDTOs/Create
        public async Task<IActionResult> Create()
        {
            var brands = await _brandService.GetAllBrandAsync("", 1) ?? new OdataResponse<ReadBrandDTOs>();
            var categories = await _categoryService.GetAllCategoryAsync("", 1) ?? new OdataResponse<ReadCategoryDTOs>();
            ViewBag.Brands = new SelectList(brands.Values, "BrandId", "BrandName");
            ViewBag.Categories = new SelectList(categories.Values, "CategoryId", "CategoryName");

            return PartialView("~/Views/AdminViews/Product/_Create.cshtml");
        }

        // POST: ReadProdutDTOs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductDTO model)
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
       .SelectMany(v => v.Errors)
       .Select(e => e.ErrorMessage)
       .ToList();

                Console.WriteLine(string.Join(",", errors));
                var category = await _categoryService.GetCategoryByIdAsync(model.CategoryId);

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
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var updateProduct = await _productService.GetProductByIdAsync(id);
            if (updateProduct == null)
            {
                return NotFound();
            }
            return PartialView("~/Views/AdminViews/Product/_Edit.cshtml", updateProduct);
        }

        // POST: ReadProdutDTOs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid ProductId, UpdateProductDTO model)
        {
            if (!ModelState.IsValid)
            {
                var brands = await _brandService.GetAllBrandAsync("", 1);
                var categories = await _categoryService.GetAllCategoryAsync("", 1);

                ViewBag.Brands = new SelectList(brands.Values, "BrandId", "BrandName");
                ViewBag.Categories = new SelectList(categories.Values, "CategoryId", "CategoryName");
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
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _productService.GetProductByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return PartialView("~/Views/AdminViews/Product/_Delete.cshtml", model);
        }

        // POST: ReadProdutDTOs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product != null)
            {
                await _productService.DeleteProductAsync(id);
            }

            return Json(new { success = true });
        }
    }

}
