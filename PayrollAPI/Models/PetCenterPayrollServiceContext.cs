using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PayrollAPI.Models;

public partial class PetCenterPayrollServiceContext : DbContext
{
    public PetCenterPayrollServiceContext()
    {
    }

    public PetCenterPayrollServiceContext(DbContextOptions<PetCenterPayrollServiceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Payroll> Payrolls { get; set; }

    public virtual DbSet<StaffSalary> StaffSalaries { get; set; }

    public virtual DbSet<Violation> Violations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=127.0.0.1,1433;Database=PetCenter_PayrollService;User Id=sa;Password=123456;TrustServerCertificate=True;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasKey(e => e.PayrollId).HasName("PK__Payroll__99DFC69243900DEB");

            entity.ToTable("Payroll");

            entity.HasIndex(e => new { e.SalaryId, e.PayrollPeriod }, "UQ__Payroll__A848BBE954D4A10A").IsUnique();

            entity.Property(e => e.PayrollId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("PayrollID");
            entity.Property(e => e.Allowance).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.BaseSalary).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Bonus).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FinalSalary).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SalaryId).HasColumnName("SalaryID");
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Salary).WithMany(p => p.Payrolls)
                .HasForeignKey(d => d.SalaryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payroll_StaffSalary");
        });

        modelBuilder.Entity<StaffSalary>(entity =>
        {
            entity.HasKey(e => e.SalaryId).HasName("PK__StaffSal__4BE204B70A8F50AF");

            entity.ToTable("StaffSalary");

            entity.Property(e => e.SalaryId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("SalaryID");
            entity.Property(e => e.BaseMonthlySalary).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
        });

        modelBuilder.Entity<Violation>(entity =>
        {
            entity.HasKey(e => e.ViolationId).HasName("PK__Violatio__18B6DC28625C7A28");

            entity.Property(e => e.ViolationId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ViolationID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Penalty).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.ViolationDate).HasColumnType("datetime");
            entity.Property(e => e.ViolationType)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasMany(d => d.Payrolls).WithMany(p => p.Violations)
                .UsingEntity<Dictionary<string, object>>(
                    "PayrollViolation",
                    r => r.HasOne<Payroll>().WithMany()
                        .HasForeignKey("PayrollId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PayrollViolation_Payroll"),
                    l => l.HasOne<Violation>().WithMany()
                        .HasForeignKey("ViolationId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PayrollViolation_Violations"),
                    j =>
                    {
                        j.HasKey("ViolationId", "PayrollId");
                        j.ToTable("PayrollViolation");
                        j.IndexerProperty<Guid>("ViolationId").HasColumnName("ViolationID");
                        j.IndexerProperty<Guid>("PayrollId").HasColumnName("PayrollID");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
