using System;
using System.Collections.Generic;

namespace OrdersAPI.Models;

public partial class OrderProductSnapshot
{
    public Guid ProductSnapshotId { get; set; }

    public Guid OrderDetailsId { get; set; }

    public string ProductName { get; set; } = null!;

    public string ProductCategory { get; set; } = null!;

    public string ProductBrand { get; set; } = null!;

    public decimal ProductPrice { get; set; }

    public virtual OrderDetail OrderDetails { get; set; } = null!;
}
