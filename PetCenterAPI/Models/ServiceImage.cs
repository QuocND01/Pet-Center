using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class ServiceImage
{
    public Guid ImageId { get; set; }

    public Guid ServiceId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? PublicId { get; set; }

    public bool? IsActive { get; set; }

    public virtual Service Service { get; set; } = null!;
}
