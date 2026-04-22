using System;
using System.Collections.Generic;

namespace ImportAPI.Models;

public partial class ImportProductSnapshot
{
    public Guid ProductSnapshotId { get; set; }

    public Guid ImportStockDetailsId { get; set; }

    public string ProductName { get; set; } = null!;

    public string ProductCategory { get; set; } = null!;

    public string ProductBrand { get; set; } = null!;

    public virtual ImportStockDetail ImportStockDetails { get; set; } = null!;
}
