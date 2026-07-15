using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class ProductImage
{
    public Guid ImageId { get; set; }

    public Guid ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public DateTime? InactiveAt { get; set; }
    public string? PublicId { get; set; }

    public bool IsActive { get; set; }

    public virtual Product Product { get; set; } = null!;
}
