using System;
using System.Collections.Generic;

namespace IdentityAPI.Models;

public partial class CategoryAttribute
{
    public Guid CategoryAttributeId { get; set; }

    public Guid CategoryId { get; set; }

    public string AttributeName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
}
