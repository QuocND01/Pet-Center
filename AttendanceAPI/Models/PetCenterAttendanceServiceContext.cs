using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AttendanceAPI.Models;

public partial class PetCenterAttendanceServiceContext : DbContext
{
    public PetCenterAttendanceServiceContext()
    {
    }

    public PetCenterAttendanceServiceContext(DbContextOptions<PetCenterAttendanceServiceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<ShiftTemplate> ShiftTemplates { get; set; }

    public virtual DbSet<StaffShift> StaffShifts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=127.0.0.1,1433;Database=PetCenter_AttendanceService;User Id=sa;Password=123456;TrustServerCertificate=True;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__8B69263C6F00BDBF");

            entity.ToTable("Attendance");

            entity.HasIndex(e => e.ShiftId, "UQ__Attendan__C0A838E0B062AD54").IsUnique();

            entity.Property(e => e.AttendanceId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("AttendanceID");
            entity.Property(e => e.CheckInTime).HasColumnType("datetime");
            entity.Property(e => e.CheckOutTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ShiftId).HasColumnName("ShiftID");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.Status).HasDefaultValue(1);

            entity.HasOne(d => d.Shift).WithOne(p => p.Attendance)
                .HasForeignKey<Attendance>(d => d.ShiftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attendance_StaffShifts");
        });

        modelBuilder.Entity<ShiftTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__ShiftTem__F87ADD07755FCF21");

            entity.Property(e => e.TemplateId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("TemplateID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<StaffShift>(entity =>
        {
            entity.HasKey(e => e.ShiftId).HasName("PK__StaffShi__C0A838E1E055D6C9");

            entity.HasIndex(e => new { e.StaffId, e.ShiftDate, e.TemplateId }, "UQ__StaffShi__06DBBF066662DAB2").IsUnique();

            entity.Property(e => e.ShiftId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ShiftID");
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.TemplateId).HasColumnName("TemplateID");

            entity.HasOne(d => d.Template).WithMany(p => p.StaffShifts)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StaffShifts_ShiftTemplates");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
