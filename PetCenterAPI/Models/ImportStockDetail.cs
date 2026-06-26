using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class ImportStockDetail
{
    public Guid ImportStockDetailsId { get; set; }

    public Guid ImportId { get; set; }

    public Guid ProductId { get; set; }

    public string SKU { get; set; } = null!;

    public string BatchCode { get; set; } = null!;
    public BatchStatus BatchStatus { get; set; } = BatchStatus.Active;

    public decimal ImportPrice { get; set; }

    public int Quantity { get; set; }

    public int StockLeft { get; set; }
    public int QuantitySold { get; set; }

    public DateOnly? ManufacturingDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public bool IsPreferredForPickup { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public virtual ImportStock Import { get; set; } = null!;

    public virtual ImportProductSnapshot? ImportProductSnapshot { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Product Product { get; set; } = null!;

}
public enum BatchStatus
{
    Active,
    Exhausted,
    Expired,
    Quarantine
}
