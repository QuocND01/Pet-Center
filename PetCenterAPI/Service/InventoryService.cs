using AutoMapper;
using ClosedXML.Excel;
using NuGet.Protocol.Core.Types;
using PetCenterAPI.DTOs.Requests.Inventory;
using PetCenterAPI.DTOs.Responses.Inventory;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using System.Drawing;

namespace PetCenterAPI.Service
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<InventoryService> _logger;
        public InventoryService(
            IInventoryRepository repo,
            IMapper mapper,
            ILogger<InventoryService> logger    )
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<InventoryListResponseDTO> GetPagedAsync(
            InventoryQueryRequestDTO request)
        {
            var (items, totalRecords) =
                await _repo.GetPagedAsync(request);

            return new InventoryListResponseDTO
            {
                Items = _mapper.Map<List<InventoryItemResponseDTO>>(items),

                TotalRecords = totalRecords,

                Page = request.Page,

                PageSize = request.PageSize,

                TotalPages = (int)Math.Ceiling(
                    totalRecords / (double)request.PageSize)
            };
        }

        public async Task<InventoryDetailResponseDTO?> GetByIdAsync(
            Guid inventoryId)
        {
            var entity = await _repo.GetByIdAsync(inventoryId);

            if (entity == null)
                return null;

            return _mapper.Map<InventoryDetailResponseDTO>(entity);
        }
        public async Task<IEnumerable<InventoryReportDto>> GetInventorySummaryAsync()
        {
            return await _repo.GetInventorySummaryReportAsync();
        }

        public async Task<IEnumerable<BatchExpiryReportDto>> GetBatchExpiryReportAsync()
        {
            return await _repo.GetBatchExpiryReportAsync();
        }
        public async Task<byte[]> GenerateInventoryExcelReportAsync()
        {
            var summaryData = await _repo.GetInventorySummaryReportAsync();
            var batchData = await _repo.GetBatchExpiryReportAsync();

            using (var workbook = new XLWorkbook())
            {
                // ==========================================
                // TAB 1: TỔNG QUAN TỒN KHO
                // ==========================================
                var ws1 = workbook.Worksheets.Add("Tổng Quan Tồn Kho");

                // Style Colors
                var navyColor = XLColor.FromHtml("#1B365D");
                var headerFontColor = XLColor.White;

                // Title Block
                ws1.Cell("A1").Value = "PET CENTER MANAGEMENT SYSTEM";
                ws1.Cell("A1").Style.Font.Bold = true;
                ws1.Cell("A1").Style.Font.FontColor = XLColor.Gray;

                ws1.Cell("A2").Value = "BÁO CÁO TỔNG QUAN TỒN KHO & GIÁ TRỊ TÀI SẢN";
                ws1.Cell("A2").Style.Font.Bold = true;
                ws1.Cell("A2").Style.Font.FontSize = 16;
                ws1.Cell("A2").Style.Font.FontColor = navyColor;

                ws1.Cell("A3").Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";

                // Headers
                string[] headers1 = { "Mã SKU", "Tên Sản Phẩm", "Tồn Sẵn Sàng (Available)", "Đang Đặt (Reserved)", "Giá Nhập Bình Quân", "Tổng Giá Trị Tồn", "Cập Nhật Cuối" };
                for (int i = 0; i < headers1.Length; i++)
                {
                    var cell = ws1.Cell(5, i + 1);
                    cell.Value = headers1[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = navyColor;
                    cell.Style.Font.FontColor = headerFontColor;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Data Rows
                int row1 = 6;
                foreach (var item in summaryData)
                {
                    ws1.Cell(row1, 1).Value = item.SKU;
                    ws1.Cell(row1, 2).Value = item.ProductName;
                    ws1.Cell(row1, 3).Value = item.QuantityAvailable;
                    ws1.Cell(row1, 4).Value = item.QuantityReserved;

                    ws1.Cell(row1, 5).Value = item.AvgImportPrice;
                    ws1.Cell(row1, 5).Style.NumberFormat.Format = "#,##0 ₫";

                    ws1.Cell(row1, 6).FormulaA1 = $"=C{row1}*E{row1}";
                    ws1.Cell(row1, 6).Style.NumberFormat.Format = "#,##0 ₫";

                    ws1.Cell(row1, 7).Value = item.LastUpdated.ToString("dd/MM/yyyy HH:mm");

                    // Alignment
                    ws1.Cell(row1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws1.Cell(row1, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws1.Cell(row1, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws1.Cell(row1, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    row1++;
                }

                // Total Row
                ws1.Cell(row1, 1).Value = "TỔNG CỘNG";
                ws1.Cell(row1, 1).Style.Font.Bold = true;
                ws1.Cell(row1, 3).FormulaA1 = $"=SUM(C6:C{row1 - 1})";
                ws1.Cell(row1, 4).FormulaA1 = $"=SUM(D6:D{row1 - 1})";
                ws1.Cell(row1, 6).FormulaA1 = $"=SUM(F6:F{row1 - 1})";
                ws1.Cell(row1, 6).Style.NumberFormat.Format = "#,##0 ₫";
                ws1.Range(row1, 1, row1, 7).Style.Font.Bold = true;

                ws1.Columns().AdjustToContents();

                // ==========================================
                // TAB 2: LÔ HÀNG & HẠN SỬ DỤNG
                // ==========================================
                var ws2 = workbook.Worksheets.Add("Lô Hàng & Hạn Sử Dụng");

                ws2.Cell("A1").Value = "PET CENTER MANAGEMENT SYSTEM";
                ws2.Cell("A1").Style.Font.Bold = true;
                ws2.Cell("A1").Style.Font.FontColor = XLColor.Gray;

                ws2.Cell("A2").Value = "CHI TIẾT LÔ HÀNG & CẢNH BÁO HẠN SỬ DỤNG (FEFO)";
                ws2.Cell("A2").Style.Font.Bold = true;
                ws2.Cell("A2").Style.Font.FontSize = 16;
                ws2.Cell("A2").Style.Font.FontColor = navyColor;

                string[] headers2 = { "Mã Lô", "Số Hóa Đơn", "Mã SKU", "Tên Sản Phẩm", "Giá Nhập", "Tồn Lô (StockLeft)", "NSX", "HSD", "Trạng Thái", "Cảnh Báo Date" };
                for (int i = 0; i < headers2.Length; i++)
                {
                    var cell = ws2.Cell(4, i + 1);
                    cell.Value = headers2[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = navyColor;
                    cell.Style.Font.FontColor = headerFontColor;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                int row2 = 5;
                foreach (var b in batchData)
                {
                    ws2.Cell(row2, 1).Value = b.BatchCode;
                    ws2.Cell(row2, 2).Value = b.InvoiceNumber;
                    ws2.Cell(row2, 3).Value = b.SKU;
                    ws2.Cell(row2, 4).Value = b.ProductName;
                    ws2.Cell(row2, 5).Value = b.ImportPrice;
                    ws2.Cell(row2, 5).Style.NumberFormat.Format = "#,##0 ₫";
                    ws2.Cell(row2, 6).Value = b.StockLeft;
                    ws2.Cell(row2, 7).Value = b.ManufacturingDate?.ToString("dd/MM/yyyy") ?? "-";
                    ws2.Cell(row2, 8).Value = b.ExpiryDate?.ToString("dd/MM/yyyy") ?? "-";
                    ws2.Cell(row2, 9).Value = b.Status;
                    ws2.Cell(row2, 10).Value = b.ExpiryAlert;

                    // Highlight cảnh báo
                    if (b.ExpiryAlert == "ĐÃ HẾT HẠN")
                    {
                        ws2.Cell(row2, 10).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8D7DA");
                        ws2.Cell(row2, 10).Style.Font.FontColor = XLColor.FromHtml("#842029");
                        ws2.Cell(row2, 10).Style.Font.Bold = true;
                    }
                    else if (b.ExpiryAlert == "SẮP HẾT HẠN")
                    {
                        ws2.Cell(row2, 10).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFF3CD");
                        ws2.Cell(row2, 10).Style.Font.FontColor = XLColor.FromHtml("#664D03");
                        ws2.Cell(row2, 10).Style.Font.Bold = true;
                    }

                    row2++;
                }

                ws2.Columns().AdjustToContents();

                // Render File Stream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
        public async Task ProcessExpiredBatchesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("--> [Inventory Check] Đang kiểm tra HSD lô hàng...");

            int count = await _repo.UpdateExpiredBatchesAsync(cancellationToken);

            if (count > 0)
                _logger.LogInformation($"--> [Inventory Check] Đã cập nhật {count} lô hàng sang trạng thái Expired.");
            else
                _logger.LogInformation("--> [Inventory Check] Không có lô hàng nào hết hạn.");
        }
    }
}