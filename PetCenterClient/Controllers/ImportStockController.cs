using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PetCenterClient.ViewModels;
using PetCenterClient.Services;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.Supplier;

namespace PetCenterClient.Controllers
{
    public class ImportStockController : Controller
    {
        private readonly IImportStockService _service;
        private readonly ISupplierApiService _suppService;
        private readonly IProductAPIClient _productService;
        private readonly IStaffService _staffService;
        private readonly ILogger<ImportStockController> _logger;
        private readonly ExcelService _excelService;

        public ImportStockController(IImportStockService service, ILogger<ImportStockController> logger, ISupplierApiService suppService, ExcelService excelService, IProductAPIClient productService, IStaffService staffService)
        {
            _service = service;
            _logger = logger;
            _suppService = suppService;
            _productService = productService;
            _staffService = staffService;
            _excelService = excelService;
        }



        public async Task<IActionResult> Index(string sortOrder)
        {
            // 1. Gọi API lấy toàn bộ danh sách dữ liệu về Client
            var imports = await _service.GetAllAsync();

            // 2. Thiết lập trạng thái Sort cho View (Rút gọn để chuyển đổi trạng thái click)
            ViewBag.IdSort = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewBag.DateSort = sortOrder == "date" ? "date_desc" : "date";
            ViewBag.TotalSort = sortOrder == "total" ? "total_desc" : "total";
            ViewBag.StatusSort = sortOrder == "status" ? "status_desc" : "status";

            // 3. Thực hiện sắp xếp trên RAM của ứng dụng Client (Sử dụng IEnumerable để tránh nhân bản danh sách)
            IEnumerable<ImportHeaderViewModel> sortedImports = sortOrder switch
            {
                "id_desc" => imports.OrderByDescending(i => i.ImportId),
                "date" => imports.OrderBy(i => i.ImportDate),
                "date_desc" => imports.OrderByDescending(i => i.ImportDate),
                "total" => imports.OrderBy(i => i.TotalAmount),
                "total_desc" => imports.OrderByDescending(i => i.TotalAmount),
                "status" => imports.OrderBy(i => i.Status),
                "status_desc" => imports.OrderByDescending(i => i.Status),

                // Mặc định: Đơn hàng mới nhập gần nhất sẽ luôn hiển thị ở trên cùng
                _ => imports.OrderByDescending(i => i.ImportDate)
            };

            // Khuyên dùng: Nếu file View nằm đúng cấu trúc thư mục chuẩn (Views/ImportStock/Index.cshtml), 
            // bạn chỉ cần: return View(sortedImports.ToList()); 
            // Dưới đây giữ nguyên đường dẫn tường minh của bạn để tránh lỗi cấu hình:
            return View("~/Views/AdminViews/ImportStock/Index.cshtml", sortedImports.ToList());
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var import = await _service.GetByIdAsync(id);

            return View("~/Views/AdminViews/ImportStock/Details.cshtml",import);
        }

        public async Task<IActionResult> Create()
        {
            var suppliers = await _suppService.GetAllAsync();

            if (suppliers == null)
                suppliers = new List<ViewSupplierViewModel>();

            ViewBag.SupplierList = new SelectList(suppliers, "SupplierId", "SupplierName");


            return View("~/Views/AdminViews/ImportStock/Create.cshtml", new CreateImportViewModel ());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateImportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var suppliers = await _suppService.GetAllAsync() ?? new List<ViewSupplierViewModel>();
                ViewBag.SupplierList = new SelectList(suppliers, "SupplierId", "SupplierName");

                TempData["Error"] = "Invalid data.";

                return View("~/Views/AdminViews/ImportStock/Create.cshtml", model);
            }

            try
            {
                await _service.CreateAsync(model);

                TempData["Success"] = "Create import request successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                var suppliers = await _suppService.GetAllAsync() ?? new List<ViewSupplierViewModel>();
                ViewBag.SupplierList = new SelectList(suppliers, "SupplierId", "SupplierName");

                return View("~/Views/AdminViews/ImportStock/Create.cshtml", model);
            }
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
        //public async Task<IActionResult> ExportExcel(DateTime? fromDate, DateTime? toDate) ok

        //{
        //    var imports = await _service.GetAllByTimeAsync();

        //    // gọi song song
        //    var supplierTask = _suppService.GetSupplierSelectAsync();
        //    var productTask = _productService.GetProductSelectAsync();
        //    var staffTask = _staffService.GetStaffNameListAsync();

        //    await Task.WhenAll(supplierTask, productTask, staffTask);

        //    var supplierDict = supplierTask.Result.ToDictionary(x => x.SupplierId, x => x.SupplierName);
        //    var productDict = productTask.Result.ToDictionary(x => x.ProductId, x => x.ProductName);
        //    var staffDict = staffTask.Result.ToDictionary(x => x.StaffId, x => x.StaffName);

        //    // filter date
        //    if (fromDate.HasValue)
        //        imports = imports.Where(x => x.ImportDate >= fromDate.Value).ToList();

        //    if (toDate.HasValue)
        //        imports = imports.Where(x => x.ImportDate <= toDate.Value).ToList();

        //    var exportData = imports.Select(i =>
        //    {
        //        supplierDict.TryGetValue(i.SupplierId, out var supplierName);
        //        staffDict.TryGetValue(i.StaffId, out var staffName);

        //        return new ImportStockExcelDto
        //        {
        //            Code = i.ImportId.ToString(),
        //            SupplierName = supplierName ?? i.SupplierName ?? "Unknown",
        //            StaffName = staffName ?? "Unknown",
        //            TotalAmount = i.TotalAmount,
        //            ImportDate = i.ImportDate,
        //            Status = i.Status.ToString(),

        //            Details = i.Details?.Select(d =>
        //            {
        //                productDict.TryGetValue(d.ProductId, out var productName);

        //                return new ImportStockDetailExcelDto
        //                {
        //                    ProductName = productName ?? "Unknown",
        //                    Quantity = d.Quantity,
        //                    ImportPrice = d.ImportPrice,
        //                    StockLeft = d.StockLeft
        //                };
        //            }).ToList() ?? new List<ImportStockDetailExcelDto>()
        //        };
        //    }).ToList();

        //    var fileBytes =  _excelService.ExportExcel(exportData);

        //    return File(
        //        fileBytes,
        //        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //        "ImportStocks.xlsx"
        //    );
        //}
    }
}
