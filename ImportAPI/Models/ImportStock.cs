using System;
using System.Collections.Generic;

namespace ImportAPI.Models;

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

    public virtual Supplier Supplier { get; set; } = null!;
    public enum ImportStatus
    {
        Pending = 0,
        Confirmed = 1,
        Cancelled = 2
    }
}
