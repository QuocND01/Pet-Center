using System;
using System.Collections.Generic;

namespace FeedbackAPI.Models;

public partial class ProductAttribute
{
    public Guid ProductAttributeId { get; set; }

    public Guid? ProductId { get; set; }

    public Guid? CategoryAttributeId { get; set; }

    public string? AttributeValue { get; set; }

    public bool? IsActive { get; set; }

    public virtual CategoryAttribute? CategoryAttribute { get; set; }

    public virtual Product? Product { get; set; }
}
