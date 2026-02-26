using System;
using System.Collections.Generic;

namespace Pet_Center.Models;

public partial class Image
{
    public Guid ImageId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
