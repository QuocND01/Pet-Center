using System;
using System.Collections.Generic;

namespace FeedbackAPI.Models;

public partial class Role
{
    public Guid RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
}
