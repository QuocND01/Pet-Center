using FeedbackAPI.Models;
using Microsoft.EntityFrameworkCore;

public class PetCenterContext : DbContext
{
    public PetCenterContext(DbContextOptions<PetCenterContext> options)
        : base(options)
    {
    }

    public DbSet<ProductFeedback> ProductFeedbacks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductFeedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId);

            entity.Property(e => e.FeedbackId)
                .HasColumnName("FeedbackID");

            entity.Property(e => e.CustomerId)
                .HasColumnName("CustomerID");

            entity.Property(e => e.ProductId)
                .HasColumnName("ProductID");

            entity.Property(e => e.OrderId)
                .HasColumnName("OrderID");

            entity.Property(e => e.StaffId)
                .HasColumnName("StaffID");

            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime");

            entity.Property(e => e.ReplyDate)
                .HasColumnType("datetime");
        });
    }
}