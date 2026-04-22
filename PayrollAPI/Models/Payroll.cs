using System;
using System.Collections.Generic;

namespace PayrollAPI.Models;

public partial class Payroll
{
    public Guid PayrollId { get; set; }

    public Guid SalaryId { get; set; }

    public DateOnly PayrollPeriod { get; set; }

    public decimal BaseSalary { get; set; }

    public decimal FinalSalary { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public decimal? Bonus { get; set; }

    public decimal? Allowance { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual StaffSalary Salary { get; set; } = null!;

    public virtual ICollection<Violation> Violations { get; set; } = new List<Violation>();
}
