using System;
using System.Collections.Generic;

namespace ProductAPI.Models;

public partial class Product
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal ProductPrice { get; set; }

    public string? ProductDescription { get; set; }

    public Guid BrandId { get; set; }

    public Guid CategoryId { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.Now;

    public DateTime? UpdateAt { get; set; }

    public bool IsActive { get; set; } = true;


    public virtual Brand? Brand { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
}
