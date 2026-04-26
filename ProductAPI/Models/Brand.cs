using System;
using System.Collections.Generic;

namespace ProductAPI.Models;

public partial class Brand
{
    public Guid BrandId { get; set; }

    public string BrandName { get; set; } = null!;

    public string? BrandLogo { get; set; }

    public bool IsActive { get; set; } = true;


    public string? BrandDescription { get; set; }
    public string? PublicId { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
