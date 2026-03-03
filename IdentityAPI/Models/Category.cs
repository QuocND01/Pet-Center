using System;
using System.Collections.Generic;

namespace IdentityAPI.Models;

public partial class Category
{
    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? CategoryLogo { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<CategoryAttribute> CategoryAttributes { get; set; } = new List<CategoryAttribute>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
