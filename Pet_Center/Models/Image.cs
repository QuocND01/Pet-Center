using System;
using System.Collections.Generic;

namespace ProductAPI.Models;

public partial class Image
{
    public Guid ImageId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public Guid ProductId { get; set; }   // FK

    public string PublicId { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual Product Product { get; set; } = null!;  // ❗ chỉ 1 Product
}
