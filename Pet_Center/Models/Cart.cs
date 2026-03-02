using System;
using System.Collections.Generic;

namespace ProductAPI.Models;

public partial class Cart
{
    public Guid CartId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
