using System;
using System.Collections.Generic;

namespace AddressAPI.Models;

public partial class OrderDetail
{
    public Guid OrderDetailId { get; set; }

    public Guid? OrderId { get; set; }

    public Guid? ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public Guid? ImportStockDetailId { get; set; }

    public virtual ImportStockDetail? ImportStockDetail { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Product? Product { get; set; }
}
