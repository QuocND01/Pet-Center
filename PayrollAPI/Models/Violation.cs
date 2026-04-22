using System;
using System.Collections.Generic;

namespace PayrollAPI.Models;

public partial class Violation
{
    public Guid ViolationId { get; set; }

    public Guid StaffId { get; set; }

    public string ViolationType { get; set; } = null!;

    public decimal Penalty { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime ViolationDate { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? Status { get; set; }

    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
}
