using System;
using System.Collections.Generic;

namespace CartAPI.Models;

public partial class Cart
{
    public Guid CartId { get; set; }

    public Guid CustomerId { get; set; }

    public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();
}
