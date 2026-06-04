using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class FeedbackImage
{
    public Guid ImageId { get; set; }

    public Guid FeedbackId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? PublicId { get; set; }

    public bool? IsActive { get; set; }

    public virtual ProductFeedback Feedback { get; set; } = null!;
}
