using PetCenterAPI.Common;
using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class Product
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal ProductPrice { get; set; }

    public string? ProductDescription { get; set; }

    public Guid BrandId { get; set; }

    public Guid CategoryId { get; set; }

    public DateTime? AddedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public Status Status { get; set; }

    public virtual Brand Brand { get; set; } = null!;

    public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<ImportStockDetail> ImportStockDetails { get; set; } = new List<ImportStockDetail>();

    public virtual Inventory? Inventory { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();

    public virtual ICollection<ProductFeedback> ProductFeedbacks { get; set; } = new List<ProductFeedback>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
