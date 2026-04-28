namespace CartAPI.DTOs;

/// <summary>Minimal product info fetched from ProductAPI</summary>
public class ProductInfoDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal ProductPrice { get; set; }
    public int StockQuantity { get; set; }
    public List<string>? Images { get; set; }
}