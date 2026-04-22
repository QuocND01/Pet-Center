using System;
using System.Collections.Generic;

namespace CartAPI.Models;

public partial class CartDetail
{
    public Guid CartDetailsId { get; set; }

    public Guid? CartId { get; set; }

    public int? Quantity { get; set; }

    public Guid? ProductId { get; set; }

    public virtual Cart? Cart { get; set; }
}
