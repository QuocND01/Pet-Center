using System;
using System.Collections.Generic;

namespace OrdersAPI.Models;

public partial class OrderDetail
{
    public Guid OrderDetailsId { get; set; }

    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public string? ImportStockDetailsId { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual OrderProductSnapshot? OrderProductSnapshot { get; set; }
}
