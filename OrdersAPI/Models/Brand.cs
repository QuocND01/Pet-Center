using System;
using System.Collections.Generic;

namespace OrdersAPI.Models;

public partial class Brand
{
    public Guid BrandId { get; set; }

    public string BrandName { get; set; } = null!;

    public string? BrandLogo { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
