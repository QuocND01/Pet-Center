using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class ImportStocksController : Controller
    {
        private readonly IImportStockService _service;
        private readonly ISupplierService _suppService;
        private readonly IProductService _productService;
        private readonly ILogger<ImportStocksController> _logger;
        
        public ImportStocksController(IImportStockService service, ILogger<ImportStocksController> logger, ISupplierService suppService, IProductService productService)
        {
            _service = service;
            _logger = logger;
            _suppService = suppService;
            _productService = productService;
        }


       
        public async Task<IActionResult> Index(string sortOrder)
        {
            var imports = await _service.GetAllAsync();
            
            ViewBag.IdSort = String.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewBag.DateSort = sortOrder == "date" ? "date_desc" : "date";
            ViewBag.TotalSort = sortOrder == "total" ? "total_desc" : "total";
            ViewBag.StatusSort = sortOrder == "status" ? "status_desc" : "status";

            imports = sortOrder switch
            {
                "id_desc" => imports.OrderByDescending(i => i.ImportId).ToList(),

                "date" => imports.OrderBy(i => i.ImportDate).ToList(),
                "date_desc" => imports.OrderByDescending(i => i.ImportDate).ToList(),

                "total" => imports.OrderBy(i => i.TotalAmount).ToList(),
                "total_desc" => imports.OrderByDescending(i => i.TotalAmount).ToList(),

                "status" => imports.OrderBy(i => i.Status).ToList(),
                "status_desc" => imports.OrderByDescending(i => i.Status).ToList(),

                _ => imports.OrderBy(i => i.ImportDate).ToList()
            };
            _logger.LogInformation("Dữ liệu nhận được: {@Imports}", imports);

            return View("~/Views/AdminViews/ImportStock/Index.cshtml",imports);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var import = await _service.GetByIdAsync(id);

            if (import == null)
                return NotFound();

            return View("~/Views/AdminViews/ImportStock/Details.cshtml",import);
        }

        public async Task<IActionResult> Create()
        {   

            var selectDto = await _suppService.GetSupplierSelectAsync();
            var products = await _productService.GetProductSelectAsync();

            ViewBag.SupplierList = new SelectList(selectDto, "SupplierId", "SupplierName");
            ViewBag.ProductList = new SelectList(products, "ProductId", "ProductName");
            return View("~/Views/AdminViews/ImportStock/Create.cshtml", new CreateImportStockDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateImportStockDto dto)
        {
            if (!ModelState.IsValid)
            {
                var selectDto = await _suppService.GetSupplierSelectAsync();
                var products = await _productService.GetProductSelectAsync();

                ViewBag.SupplierList = new SelectList(selectDto, "SupplierId", "SupplierName");
                ViewBag.ProductList = new SelectList(products, "ProductId", "ProductName");

                return View("~/Views/AdminViews/ImportStock/Create.cshtml", dto);
            }

            await _service.CreateAsync(dto);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Confirm(Guid id)
        {
            await _service.ConfirmAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Cancel(Guid id)
        {
            await _service.CancelAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
