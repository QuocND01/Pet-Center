using System;
using System.Collections.Generic;

namespace PayrollAPI.Models;

public partial class StaffSalary
{
    public Guid SalaryId { get; set; }

    public Guid StaffId { get; set; }

    public decimal BaseMonthlySalary { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
}
