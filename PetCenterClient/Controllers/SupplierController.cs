using Microsoft.AspNetCore.Mvc;
using PetCenterClient.ViewModels;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.Supplier;

namespace PetCenterClient.Controllers
{
    public class SupplierController : Controller
    {
        private readonly ISupplierApiService _supplierService;

        public SupplierController(ISupplierApiService supplierService)
        {
            _supplierService = supplierService;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index()
        {
            var suppliers = await _supplierService.GetAllAsync();

            if (suppliers == null)
                suppliers = new List<ViewSupplierViewModel>();

            return View("~/Views/AdminViews/Supplier/Index.cshtml", suppliers);
        }

        // GET: Create
        public IActionResult Create()
        {   
            
            return View("~/Views/AdminViews/Supplier/Create.cshtml");

        }

        // POST: Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateSupplierViewModel dto)
        {
            // 1. Validate Form ở phía Client ViewModel
            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/AdminViews/Supplier/Create.cshtml", dto);
            }

            try
            {
                // 2. Gọi Service gửi request sang API
                var result = await _supplierService.CreateAsync(dto);

                // 3. Kiểm tra kết quả phản hồi từ API
                if (result != null && result.Status)
                {
                    return Json(new
                    {
                        success = true,
                        message = result.Message // Thông báo thành công từ API
                    });
                }

                // 4. API trả về thất bại (Trùng TaxId, Lỗi nghiệp vụ...)
                return Json(new
                {
                    success = false,
                    message = result?.Message ?? "Không thể tạo nhà cung cấp. Vui lòng thử lại!"
                });
            }
            catch (Exception ex)
            {
                // 5. Bắt các lỗi hệ thống không lường trước (Mất kết nối mạng, sập server...)
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi kết nối đến máy chủ: " + ex.Message
                });
            }
        }

        // GET: Edit
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);

            if (supplier == null)
                return NotFound();

            var dto = new CreateSupplierViewModel
            {
                SupplierId = id,
                TaxId = supplier.TaxId,
                SupplierName = supplier.SupplierName,
                SupplierEmail = supplier.SupplierEmail,
                SupplierPhoneNumber = supplier.SupplierPhoneNumber,
                SupplierAddress = supplier.SupplierAddress,
                ContactPerson = supplier.ContactPerson,
                SupplierDescription = supplier.SupplierDescription
            };

            return PartialView("~/Views/AdminViews/Supplier/Edit.cshtml", dto);
        }

        // POST: Edit
        [HttpPost]
        public async Task<IActionResult> Edit(CreateSupplierViewModel dto)
        {
            // 1. Kiểm tra validation ở ViewModel
            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/AdminViews/Supplier/Edit.cshtml", dto);
            }

            try
            {
                // 2. Gọi Service gửi dữ liệu cập nhật
                var result = await _supplierService.UpdateAsync(dto.SupplierId, dto);

                // 3. Kiểm tra kết quả trả về từ API
                if (result != null && result.Status)
                {
                    return Json(new
                    {
                        success = true,
                        message = result.Message ?? "Supplier updated successfully."
                    });
                }

                // 4. Khi API trả về thất bại (Ví dụ: trùng TaxId, không tìm thấy Supplier...)
                return Json(new
                {
                    success = false,
                    message = result?.Message ?? "Error updating supplier."
                });
            }
            catch (Exception ex)
            {
                // 5. Bắt các ngoại lệ ngoài ý muốn (sập server API, mất mạng...)
                return Json(new
                {
                    success = false,
                    message = "An error occurred: " + ex.Message
                });
            }
        }

        // DELETE
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _supplierService.DeleteAsync(id);

                return Json(new
                {
                    success = result,
                    message = result
                        ? "Supplier deleted successfully."
                        : "Failed to delete supplier."
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