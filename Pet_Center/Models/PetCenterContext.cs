using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProductAPI.Models;

public partial class PetCenterContext : DbContext
{
    public PetCenterContext()
    {
    }

    public PetCenterContext(DbContextOptions<PetCenterContext> options)
        : base(options)
    {
    }

   

    public virtual DbSet<Brand> Brands { get; set; }

   

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryAttribute> CategoryAttributes { get; set; }


    public virtual DbSet<Image> Images { get; set; }


    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductAttribute> ProductAttributes { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__Brands__DAD4F3BE099C4489");

            entity.Property(e => e.BrandId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("BrandID");
            entity.Property(e => e.BrandLogo).HasMaxLength(255);
            entity.Property(e => e.BrandName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });


        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2B81FE1694");

            entity.Property(e => e.CategoryId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("CategoryID");
            entity.Property(e => e.CategoryLogo).HasMaxLength(255);
            entity.Property(e => e.CategoryName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<CategoryAttribute>(entity =>
        {
            entity.HasKey(e => e.CategoryAttributeId).HasName("PK__Category__21D38D92175E55BB");

            entity.Property(e => e.CategoryAttributeId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("CategoryAttributeID");
            entity.Property(e => e.AttributeName).HasMaxLength(150);
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryAttributes)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CategoryA__Categ__6D0D32F4");
        });

       

       

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.ImageId);

            entity.Property(e => e.ImageId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ImageID");

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500);

            entity.Property(e => e.PublicId)
                .HasMaxLength(50);

            entity.Property(e => e.ProductId)
                .HasColumnName("ProductID");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.HasOne(d => d.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6ED2A906317");

            entity.HasIndex(e => new { e.ProductName, e.BrandId, e.CategoryId })
         .IsUnique();

            entity.Property(e => e.ProductId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ProductID");
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.BrandId).HasColumnName("BrandID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.ProductPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK__Products__BrandI__5EBF139D");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Products__Catego__5FB337D6");

            entity.Property(e => e.ProductId)
                .HasColumnName("ProductID");

        });

        modelBuilder.Entity<ProductAttribute>(entity =>
        {
            entity.HasKey(e => e.ProductAttributeId).HasName("PK__ProductA__00CE6727479B0F94");

            entity.HasIndex(e => new { e.ProductId, e.CategoryAttributeId }, "UQ__ProductA__8611FE35D914D793").IsUnique();

            entity.Property(e => e.ProductAttributeId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ProductAttributeID");
            entity.Property(e => e.AttributeValue).HasMaxLength(255);
            entity.Property(e => e.CategoryAttributeId).HasColumnName("CategoryAttributeID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.CategoryAttribute).WithMany(p => p.ProductAttributes)
                .HasForeignKey(d => d.CategoryAttributeId)
                .HasConstraintName("FK__ProductAt__Categ__73BA3083");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductAttributes)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductAt__Produ__72C60C4A");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
