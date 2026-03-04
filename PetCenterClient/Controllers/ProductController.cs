using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCenterClient.Services.Interface;
using ProductAPI.DTOs;

namespace PetCenterClient.Controllers
{
    public class ReadProdutDTOsController : Controller
    {
        private readonly IProductService _productService;

        public ReadProdutDTOsController(IProductService productService)
        {
            _productService = productService;
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
               search, isActive, minPrice, maxPrice, fromDate, toDate, sortBy, sortOrder, pagesize);
            var totalItems = result.Count;
            var totalPages = (int)Math.Ceiling((decimal)(totalItems / (decimal)pagesize));
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            return View(result);
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

            return View(readProdutDTOs);
        }

        // GET: ReadProdutDTOs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ReadProdutDTOs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _productService.AddProductAsync(model);

            return RedirectToAction(nameof(Index));
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
            return View(updateProduct);
        }

        // POST: ReadProdutDTOs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateProductDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _productService.UpdateProductAsync(id, model);

            return RedirectToAction(nameof(Index));
        }

        // GET: ReadProdutDTOs/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deleteProdutDTOs = await _productService.GetProductByIdAsync(id);
            if (deleteProdutDTOs == null)
            {
                return NotFound();
            }

            return View(deleteProdutDTOs);
        }

        // POST: ReadProdutDTOs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var deleteProdutDTOs = await _productService.GetProductByIdAsync(id);
            if (deleteProdutDTOs != null)
            {
                await _productService.DeleteProductAsync(id);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ReadProdutDTOsExists(Guid id)
        {
            var product = _productService.GetProductByIdAsync(id);
            return product != null;

        }
    }

}
