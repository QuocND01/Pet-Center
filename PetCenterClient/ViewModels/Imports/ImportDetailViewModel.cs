namespace PetCenterClient.ViewModels
{
    public class ImportDetailViewModel
    {
        public Guid? ImportId { get; set; }
        public Guid ImportStockDetailId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public string SKU { get; set; } = null!;
        public string BatchCode { get; set; } = null!;
        public int StockLeft { get; set; }
        public DateOnly? ManufacturingDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
    }

    public class CreateImportDetailViewModel
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public string SKU { get; set; } = null!;
        public string BatchCode { get; set; } = null!;
        public DateOnly? ManufacturingDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }


    }
    
    public class IncreaseStock
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
    public class DecreaseStock
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
