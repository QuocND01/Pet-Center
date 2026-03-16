using Microsoft.EntityFrameworkCore;
using OrdersAPI.Models;

namespace OrdersAPI.Models
{
    public partial class PetCenterVoucherServiceContext : DbContext
    {
        public PetCenterVoucherServiceContext() { }

        public PetCenterVoucherServiceContext(DbContextOptions<PetCenterVoucherServiceContext> options)
            : base(options) { }

        public virtual DbSet<Voucher> Vouchers { get; set; }
        public virtual DbSet<CustomerVoucher> CustomerVouchers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.HasKey(e => e.VoucherId).HasName("PK__Vouchers__3AEE79C19E265D9C");
                entity.HasIndex(e => e.Code, "UQ__Vouchers__A25C5AA7").IsUnique();

                entity.Property(e => e.VoucherId)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("VoucherID");
                entity.Property(e => e.Code).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.MinOrderAmount).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.MaxDiscountAmount).HasColumnType("decimal(10, 2)");
            });

            modelBuilder.Entity<CustomerVoucher>(entity =>
            {
                entity.HasKey(e => new { e.CustomerId, e.VoucherId });

                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
                entity.Property(e => e.VoucherId).HasColumnName("VoucherID");
                entity.Property(e => e.IsUsed).HasDefaultValue(false);

                entity.HasOne(d => d.Voucher)
                    .WithMany(p => p.CustomerVouchers)
                    .HasForeignKey(d => d.VoucherId)
                    .HasConstraintName("FK__CustomerV__Vouch");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}