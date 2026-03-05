using System;
using System.Collections.Generic;

namespace InventoryAPI.Models;

public partial class ImportStock
{
    public Guid ImportId { get; set; }

    public Guid SupplierId { get; set; }

    public Guid StaffId { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime? ImportDate { get; set; }
    public ImportStatus Status { get; set; }
    

    public virtual ICollection<ImportStockDetail> ImportStockDetails { get; set; } = new List<ImportStockDetail>();

    public virtual Staff Staff { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;
    public enum ImportStatus
    {
        Draft = 0,
        Confirmed = 1,
        Cancelled = 2
    }
}
