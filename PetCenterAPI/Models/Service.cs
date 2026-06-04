using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class Service
{
    public Guid ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public decimal Price { get; set; }

    public int Status { get; set; }

    public string? ServiceDescription { get; set; }

    public int Duration { get; set; }

    public int ServiceType { get; set; }

    public virtual ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();

    public virtual ICollection<ServiceImage> ServiceImages { get; set; } = new List<ServiceImage>();
}
