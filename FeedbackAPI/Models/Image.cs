using System;
using System.Collections.Generic;

namespace FeedbackAPI.Models;

public partial class Image
{
    public Guid ImageId { get; set; }

    public Guid ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? PublicId { get; set; }

    public bool? IsActive { get; set; }

    public virtual Product Product { get; set; } = null!;
}
