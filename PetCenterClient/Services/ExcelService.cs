using ClosedXML.Excel;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Services
{
    public class ExcelService
    {
        private readonly IImportStockService _importService;

        public ExcelService(IImportStockService importService)
        {
            _importService = importService;
        }

        public byte[] ExportExcel(List<ImportStockExcelDto> data)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Import Stocks");

            int row = 1;

            // HEADER
            ws.Cell(row, 1).Value = "Code";
            ws.Cell(row, 2).Value = "Supplier";
            ws.Cell(row, 3).Value = "Staff";
            ws.Cell(row, 4).Value = "Total";
            ws.Cell(row, 5).Value = "Date";
            ws.Cell(row, 6).Value = "Status";

            ws.Range(row, 1, row, 6).Style.Font.Bold = true;
            ws.Range(row, 1, row, 6).Style.Fill.BackgroundColor = XLColor.LightGray;

            row++;

            foreach (var item in data ?? new List<ImportStockExcelDto>())
            {
                // MAIN ROW
                ws.Cell(row, 1).Value = item.Code;
                ws.Cell(row, 2).Value = item.SupplierName;
                ws.Cell(row, 3).Value = item.StaffName;
                ws.Cell(row, 4).Value = item.TotalAmount;
                ws.Cell(row, 5).Value = item.ImportDate.ToString("dd/MM/yyyy");
                ws.Cell(row, 6).Value = item.Status;

                row++;

                // DETAIL HEADER
                ws.Cell(row, 2).Value = "Product";
                ws.Cell(row, 3).Value = "Qty";
                ws.Cell(row, 4).Value = "Price";
                ws.Cell(row, 5).Value = "Stock Left";

                ws.Range(row, 2, row, 5).Style.Font.Bold = true;
                ws.Range(row, 2, row, 5).Style.Fill.BackgroundColor = XLColor.LightBlue;

                row++;

                // DETAILS
                foreach (var d in item.Details ?? new List<ImportStockDetailExcelDto>())
                {
                    ws.Cell(row, 2).Value = d.ProductName;
                    ws.Cell(row, 3).Value = d.Quantity;
                    ws.Cell(row, 4).Value = d.ImportPrice;
                    ws.Cell(row, 5).Value = d.StockLeft;

                    row++;
                }

                row++; // spacing giữa các import
            }

            // FORMAT
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return stream.ToArray();
        }
    }
}