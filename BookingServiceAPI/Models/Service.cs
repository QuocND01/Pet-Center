using System;
using System.Collections.Generic;

namespace BookingServiceAPI.Models;

public partial class Service
{
    public Guid ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public decimal Price { get; set; }

    public bool? IsActive { get; set; }

    public string? ServiceDescription { get; set; }

    public string Duration { get; set; } = null!;
}
