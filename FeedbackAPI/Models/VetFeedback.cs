using System;
using System.Collections.Generic;

namespace FeedbackAPI.Models;

public partial class VetFeedback
{
    public Guid VetFeedbackId { get; set; }

    public Guid? CustomerId { get; set; }

    public Guid? StaffId { get; set; }

    public int? Star { get; set; }

    public string? Comment { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsHidden { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
