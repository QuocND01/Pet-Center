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

        public InventoriesController(IInventoryService service)
        {
            _service = service;
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

        /// <summary>
        /// Tải file Excel Báo cáo kho
        /// </summary>
        [HttpGet("reports/export-excel")]
        public async Task<IActionResult> ExportExcel()
        {
            var fileBytes = await _service.GenerateInventoryExcelReportAsync();
            string fileName = $"BaoCaoKho_PetCenter_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        #endregion
    }
}
