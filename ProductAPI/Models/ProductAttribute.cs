using System;
using System.Collections.Generic;

namespace ProductAPI.Models;

public partial class ProductAttribute
{
    public Guid ProductAttributesId { get; set; }

    public Guid ProductId { get; set; }

    public Guid CategoryAttributeId { get; set; }

    public bool IsActive { get; set; } = true;

    public string AttributeValue { get; set; } = null!;

    public virtual CategoryAttribute CategoryAttribute { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
