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

        public ProductsController(IProductService productService, IBrandService brandService, ICategoryService categoryService)
        {
            _productService = productService;
            _brandService = brandService;
            _categoryService = categoryService;
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
             string sortOrder = "asc",
             int page = 1)
        {
            int pagesize = 3;
            var result = await _productService.GetAllProductAsync(
               search, isActive, minPrice, maxPrice, fromDate, toDate, sortBy, sortOrder, page);
            int totalItems = result?.Count ?? 0;
            var totalPages = (int)Math.Ceiling((decimal)(totalItems / (decimal)pagesize));
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
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
             string sortOrder = "asc",
             int page = 1)
        {
            int pagesize = 3;
            var result = await _productService.GetAllProductAsync(
               search, isActive, minPrice, maxPrice, fromDate, toDate, sortBy, sortOrder, page);
            int totalItems = result?.Count ?? 0;
            var totalPages = (int)Math.Ceiling((decimal)(totalItems / (decimal)pagesize));
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
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
            return View("~/Views/CustomerViews/Product/Details.cshtml", readProdutDTOs);
        }

        // GET: ReadProdutDTOs/Create
        public async Task<IActionResult> Create()
        {
            var brands = await _brandService.GetAllBrandAsync() ?? new List<ReadBrandDTOs>();
            var categories = await _categoryService.GetAllCategoryAsync() ?? new List<ReadCategoryDTOs>();
            Console.WriteLine("number of brand: "+ brands.Count());
            Console.WriteLine("number of category: "+ categories.Count());
            ViewBag.Brands = new SelectList(brands, "BrandId", "BrandName");
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");

            return PartialView("~/Views/AdminViews/Product/_Create.cshtml");
        }

        // POST: ReadProdutDTOs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductDTO model)
        {

            Console.WriteLine("ProductName: " + model.ProductName);
            Console.WriteLine("CategoryId: " + model.CategoryId);
            Console.WriteLine("BrandId: " + model.BrandId);

            if (model.Attributes != null)
            {
                Console.WriteLine("Attribute count: " + model.Attributes.Count);
            }
            else
            {
                Console.WriteLine("Attributes NULL");
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return PartialView("~/Views/AdminViews/Product/_Create.cshtml", model);
            }
            try
            {
                await _productService.AddProductAsync(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Json(new { success = true });
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
                return View(model);

            await _productService.UpdateProductAsync(ProductId, model);

            return Json(new { success = true });
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

        private bool ReadProdutDTOsExists(Guid id)
        {
            var product = _productService.GetProductByIdAsync(id);
            return product != null;

        }
    }

}
