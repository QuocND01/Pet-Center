using System;
using System.Collections.Generic;
using InventoryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Data;

public partial class InventoryContext : DbContext
{
    public InventoryContext(DbContextOptions<InventoryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ImportStock> ImportStocks { get; set; }

    public virtual DbSet<ImportStockDetail> ImportStockDetails { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImportStock>(entity =>
        {
            entity.HasKey(e => e.ImportId).HasName("PK__ImportSt__8697678A92BBC439");

            entity.Property(e => e.ImportId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ImportDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Supplier).WithMany(p => p.ImportStocks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ImportSto__Suppl__2B3F6F97");
        });

        modelBuilder.Entity<ImportStockDetail>(entity =>
        {
            entity.HasKey(e => e.ImportStockDetailId).HasName("PK__ImportSt__CDCE9B69E5B3B125");

            entity.Property(e => e.ImportStockDetailId).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Import).WithMany(p => p.ImportStockDetails).HasConstraintName("FK__ImportSto__Impor__2F10007B");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE66694AAC2D0AE");

            entity.Property(e => e.SupplierId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
