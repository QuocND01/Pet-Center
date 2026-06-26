using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class ImportStock
{
    public Guid ImportId { get; set; }

    public Guid SupplierId { get; set; }

    public Guid StaffId { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime ImportDate { get; set; }

    public ImportStatus Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Note { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public virtual ICollection<ImportStockDetail> ImportStockDetails { get; set; } = new List<ImportStockDetail>();

    public virtual Staff Staff { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;
    public enum ImportStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 0
    }
}