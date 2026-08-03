using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.DTOs.Requests.Inventory;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Controllers
{
    [Route("api/inventories")]
    [ApiController]
    public class InventoriesController : ControllerBase
    {
        private readonly IInventoryService _service;
        private readonly IImportStockService _import;

        public InventoriesController(IInventoryService service, IImportStockService import  )
        {
            _service = service;
            _import = import;
        }

        /// <summary>
        /// Get inventory list
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] InventoryQueryRequestDTO request)
        {
            var result = await _service.GetPagedAsync(request);

            return Ok(result);
        }

        /// <summary>
        /// Get inventory detail
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    Message = "Inventory not found."
                });
            }

            return Ok(result);
        }
        #region Inventory Reports

        /// <summary>
        /// Xem trước dữ liệu báo cáo tổng quan tồn kho (JSON)
        /// </summary>
        [HttpGet("reports/summary")]
        public async Task<IActionResult> GetSummary()
        {
            var data = await _service.GetInventorySummaryAsync();
            return Ok(data);
        }

        /// <summary>
        /// Xem trước dữ liệu chi tiết các lô hàng & HSD (JSON)
        /// </summary>
        [HttpGet("reports/batch-expiry")]
        public async Task<IActionResult> GetBatchExpiry()
        {
            var data = await _service.GetBatchExpiryReportAsync();
            return Ok(data);
        }

        [HttpGet("reports/export-excel")]
        public async Task<IActionResult> ExportExcel()
        {
            try
            {
                Console.WriteLine("1. Start Export");

                var fileBytes = await _service.GenerateInventoryExcelReportAsync();

                if (fileBytes == null || fileBytes.Length == 0)
                {
                    Console.WriteLine("❌ Error: File bytes is NULL or Empty!");
                    return BadRequest("File xuất ra bị rỗng.");
                }

                Console.WriteLine($"2. Generated: {fileBytes.Length} bytes");

                string fileName = $"BaoCaoKho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                Console.WriteLine("3. Preparing File result...");

                // Trả file dạng Byte Array chuẩn
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                // In toàn bộ chi tiết lỗi ra Console thay vì để Crash ngầm
                Console.WriteLine("🔥 CRITICAL ERROR IN EXPORT EXCEL:");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion
        //// GET: api/inventory/export
        //[HttpGet("export")]
        //public async Task<IActionResult> Export(
        //    DateTime? fromDate,
        //    DateTime? toDate)
        //{
        //    var result = await _import.Export(fromDate, toDate);

        //    return Ok(result);
        //}
    }
}
