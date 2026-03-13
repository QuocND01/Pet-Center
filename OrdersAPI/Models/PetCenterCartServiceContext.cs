using Microsoft.EntityFrameworkCore;

namespace OrdersAPI.Models;

public partial class PetCenterCartServiceContext : DbContext
{
    public PetCenterCartServiceContext()
    {
    }

    public PetCenterCartServiceContext(DbContextOptions<PetCenterCartServiceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }
    public virtual DbSet<CartDetail> CartDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.ToTable("Cart");
            entity.HasKey(e => e.CartId).HasName("PK__Cart__51BCD7972DFCD59D");

            entity.Property(e => e.CartId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("CartID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
        });

        modelBuilder.Entity<CartDetail>(entity =>
        {
            entity.ToTable("CartDetails");
            entity.HasKey(e => e.CartDetailId).HasName("PK__CartDeta__CartDetailsID");

            entity.Property(e => e.CartDetailId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("CartDetailsID");
            entity.Property(e => e.CartId).HasColumnName("CartID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartDetails)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK__CartDetai__CartI__6E01572D");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}