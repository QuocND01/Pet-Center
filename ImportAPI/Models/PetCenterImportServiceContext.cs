using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ImportAPI.Models;

public partial class PetCenterImportServiceContext : DbContext
{
    public PetCenterImportServiceContext()
    {
    }

    public PetCenterImportServiceContext(DbContextOptions<PetCenterImportServiceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ImportProductSnapshot> ImportProductSnapshots { get; set; }

    public virtual DbSet<ImportStock> ImportStocks { get; set; }

    public virtual DbSet<ImportStockDetail> ImportStockDetails { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

   
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImportProductSnapshot>(entity =>
        {
            entity.HasKey(e => e.ProductSnapshotId).HasName("PK__ImportPr__AB58F798412F9153");

            entity.ToTable("ImportProductSnapshot");

            entity.HasIndex(e => e.ImportStockDetailsId, "UQ__ImportPr__3EE53B5BC9E26D8C").IsUnique();

            entity.Property(e => e.ProductSnapshotId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ProductSnapshotID");
            entity.Property(e => e.ImportStockDetailsId).HasColumnName("ImportStockDetailsID");
            entity.Property(e => e.ProductBrand).HasMaxLength(100);
            entity.Property(e => e.ProductCategory).HasMaxLength(100);
            entity.Property(e => e.ProductName).HasMaxLength(255);

            entity.HasOne(d => d.ImportStockDetails).WithOne(p => p.ImportProductSnapshot)
                .HasForeignKey<ImportProductSnapshot>(d => d.ImportStockDetailsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImportProductSnapshot_Details");
        });

        modelBuilder.Entity<ImportStock>(entity =>
        {
            entity.HasKey(e => e.ImportId).HasName("PK__ImportSt__8697678A833A00B7");

            entity.Property(e => e.ImportId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ImportID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImportDate).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.Status)
                .HasConversion<int>() // fix to use enum for int, default is pending for new request
                .HasDefaultValue(ImportStock.ImportStatus.Pending);
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Supplier).WithMany(p => p.ImportStocks)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImportStocks_Suppliers");
        });

        modelBuilder.Entity<ImportStockDetail>(entity =>
        {
            entity.HasKey(e => e.ImportStockDetailsId).HasName("PK__ImportSt__3EE53B5ADE814564");

            entity.Property(e => e.ImportStockDetailsId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ImportStockDetailsID");
            entity.Property(e => e.ImportId).HasColumnName("ImportID");
            entity.Property(e => e.ImportPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Import).WithMany(p => p.ImportStockDetails)
                .HasForeignKey(d => d.ImportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImportStockDetails_ImportStocks");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE66694764E4EA9");

            entity.Property(e => e.SupplierId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("SupplierID");
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SupplierAddress)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SupplierDescription)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SupplierEmail)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SupplierName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SupplierPhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TaxId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TaxID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
