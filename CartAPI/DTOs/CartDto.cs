using System.ComponentModel.DataAnnotations;

namespace CartAPI.DTOs;

// ── READ ──────────────────────────────────────────────────────────
public class CartReadDto
{
    public Guid CartId { get; set; }
    public Guid CustomerId { get; set; }
    public List<CartDetailReadDto> CartDetails { get; set; } = new();
    public decimal TotalPrice { get; set; }
    public int TotalItems { get; set; }
}

public class CartDetailReadDto
{
    public Guid CartDetailsId { get; set; }
    public Guid? ProductId { get; set; }
    public int? Quantity { get; set; }

    // thông tin product (fetch từ ProductAPI)
    public string? ProductName { get; set; }
    public decimal? ProductPrice { get; set; }
    public string? ProductImage { get; set; }
    public int? StockQuantity { get; set; }
    public decimal? SubTotal { get; set; }
}

// ── ADD ITEM ──────────────────────────────────────────────────────
public class CartDetailAddDto
{
    [Required(ErrorMessage = "ProductId is required")]
    public Guid ProductId { get; set; }

    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
    public int Quantity { get; set; } = 1;
}

// ── UPDATE ITEM ───────────────────────────────────────────────────
public class CartDetailUpdateDto
{
    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
    public int Quantity { get; set; }
}

// ── INTERNAL: CustomerService gọi để khởi tạo cart ───────────────
public class CartInitDto
{
    [Required(ErrorMessage = "CustomerId is required")]
    public Guid CustomerId { get; set; }
}