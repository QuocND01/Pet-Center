using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace IdentityAPI.Models;

public partial class PetCenterContext : DbContext
{
    public PetCenterContext()
    {
    }

    public PetCenterContext(DbContextOptions<PetCenterContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryAttribute> CategoryAttributes { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerVoucher> CustomerVouchers { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<ImportStock> ImportStocks { get; set; }

    public virtual DbSet<ImportStockDetail> ImportStockDetails { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductAttribute> ProductAttributes { get; set; }

    public virtual DbSet<ProductFeedback> ProductFeedbacks { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Staff> Staffs { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=127.0.0.1,1433;Database=PetCenter;User Id=sa;Password=123456;TrustServerCertificate=True;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__Addresse__091C2A1B350957E5");

            entity.Property(e => e.AddressId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("AddressID");
            entity.Property(e => e.AddressDetails).HasMaxLength(255);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.Ward)
                .HasMaxLength(100)
                .HasColumnName("WARD");

            entity.HasOne(d => d.Customer).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Addresses__Custo__403A8C7D");
        });

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

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Cart__51BCD7975E72E1D5");

            entity.ToTable("Cart");

            entity.Property(e => e.CartId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("CartID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Customer).WithMany(p => p.Carts)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cart__CustomerID__778AC167");

            entity.HasOne(d => d.Product).WithMany(p => p.Carts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cart__ProductID__787EE5A0");
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

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B816120372");

            entity.HasIndex(e => e.Email, "IX_Customers_Email");

            entity.HasIndex(e => e.PhoneNumber, "IX_Customers_PhoneNumber");

            entity.HasIndex(e => e.Email, "UQ__Customer__A9D10534886AE6FF").IsUnique();

            entity.Property(e => e.CustomerId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("CustomerID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<CustomerVoucher>(entity =>
        {
            entity.HasKey(e => new { e.CustomerId, e.VoucherId }).HasName("PK__Customer__A7008324E867FF3E");

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.VoucherId).HasColumnName("VoucherID");
            entity.Property(e => e.IsUsed).HasDefaultValue(false);

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerVouchers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerV__Custo__01142BA1");

            entity.HasOne(d => d.Voucher).WithMany(p => p.CustomerVouchers)
                .HasForeignKey(d => d.VoucherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerV__Vouch__02084FDA");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__Images__7516F4EC16358434");

            entity.Property(e => e.ImageId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ImageID");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<ImportStock>(entity =>
        {
            entity.HasKey(e => e.ImportId).HasName("PK__ImportSt__8697678A07484436");

            entity.Property(e => e.ImportId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ImportID");
            entity.Property(e => e.ImportDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Staff).WithMany(p => p.ImportStocks)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ImportSto__Staff__07C12930");

            entity.HasOne(d => d.Supplier).WithMany(p => p.ImportStocks)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ImportSto__Suppl__06CD04F7");
        });

        modelBuilder.Entity<ImportStockDetail>(entity =>
        {
            entity.HasKey(e => e.ImportStockDetailId).HasName("PK__ImportSt__CDCE9B69A166333F");

            entity.Property(e => e.ImportStockDetailId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ImportStockDetailID");
            entity.Property(e => e.ImportId).HasColumnName("ImportID");
            entity.Property(e => e.ImportPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Import).WithMany(p => p.ImportStockDetails)
                .HasForeignKey(d => d.ImportId)
                .HasConstraintName("FK__ImportSto__Impor__0B91BA14");

            entity.HasOne(d => d.Product).WithMany(p => p.ImportStockDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ImportSto__Produ__0C85DE4D");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAF4BE2922C");

            entity.HasIndex(e => e.CustomerId, "IX_Orders_CustomerID");

            entity.HasIndex(e => e.OrderDate, "IX_Orders_OrderDate");

            entity.Property(e => e.OrderId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("OrderID");
            entity.Property(e => e.AddressId).HasColumnName("AddressID");
            entity.Property(e => e.AddressSnapshot).HasMaxLength(255);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.DeliveredDate).HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.Status).HasDefaultValue(0);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Address).WithMany(p => p.Orders)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__AddressI__1332DBDC");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__Customer__123EB7A3");

            entity.HasOne(d => d.Staff).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__Orders__StaffID__14270015");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D30CCDA4F965");

            entity.HasIndex(e => e.ProductId, "IX_OrderDetails_ProductID");

            entity.HasIndex(e => new { e.OrderId, e.ProductId }, "UQ__OrderDet__08D097C0A9BF2B4C").IsUnique();

            entity.Property(e => e.OrderDetailId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("OrderDetailID");
            entity.Property(e => e.ImportStockDetailId).HasColumnName("ImportStockDetailID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.ImportStockDetail).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ImportStockDetailId)
                .HasConstraintName("FK__OrderDeta__Impor__1AD3FDA4");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderDeta__Order__18EBB532");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__OrderDeta__Produ__19DFD96B");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6ED2A906317");

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
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK__Products__BrandI__5EBF139D");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Products__Catego__5FB337D6");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Products)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__Products__Suppli__60A75C0F");

            entity.HasMany(d => d.Images).WithMany(p => p.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductImage",
                    r => r.HasOne<Image>().WithMany()
                        .HasForeignKey("ImageId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ProductIm__Image__68487DD7"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ProductIm__Produ__6754599E"),
                    j =>
                    {
                        j.HasKey("ProductId", "ImageId").HasName("PK__ProductI__635DA9A38631CC7F");
                        j.ToTable("ProductImages");
                        j.IndexerProperty<Guid>("ProductId").HasColumnName("ProductID");
                        j.IndexerProperty<Guid>("ImageId").HasColumnName("ImageID");
                    });
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

        modelBuilder.Entity<ProductFeedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__ProductF__6A4BEDF65485B8F7");

            entity.Property(e => e.FeedbackId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("FeedbackID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsVisible).HasDefaultValue(true);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.ReplyDate).HasColumnType("datetime");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");

            entity.HasOne(d => d.Customer).WithMany(p => p.ProductFeedbacks)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductFe__Custo__22751F6C");

            entity.HasOne(d => d.Order).WithMany(p => p.ProductFeedbacks)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductFe__Order__245D67DE");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductFeedbacks)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductFe__Produ__236943A5");

            entity.HasOne(d => d.Staff).WithMany(p => p.ProductFeedbacks)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__ProductFe__Staff__25518C17");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3AFD33A555");

            entity.Property(e => e.RoleId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("RoleID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PK__Staffs__96D4AAF705FF9F0B");

            entity.HasIndex(e => e.Email, "UQ__Staffs__A9D10534B2945F26").IsUnique();

            entity.Property(e => e.StaffId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("StaffID");
            entity.Property(e => e.BirthDay).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.HiredDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(10);

            entity.HasMany(d => d.Roles).WithMany(p => p.Staff)
                .UsingEntity<Dictionary<string, object>>(
                    "StaffRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__StaffRole__RoleI__4D94879B"),
                    l => l.HasOne<Staff>().WithMany()
                        .HasForeignKey("StaffId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__StaffRole__Staff__4CA06362"),
                    j =>
                    {
                        j.HasKey("StaffId", "RoleId").HasName("PK__StaffRol__2E7B0614BEA9CA3D");
                        j.ToTable("StaffRoles");
                        j.IndexerProperty<Guid>("StaffId").HasColumnName("StaffID");
                        j.IndexerProperty<Guid>("RoleId").HasColumnName("RoleID");
                    });
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE66694E3E93050");

            entity.Property(e => e.SupplierId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("SupplierID");
            entity.Property(e => e.ContactPersion).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SupplierAddress).HasMaxLength(255);
            entity.Property(e => e.SupplierDescription).HasMaxLength(255);
            entity.Property(e => e.SupplierEmail).HasMaxLength(200);
            entity.Property(e => e.SupplierName).HasMaxLength(200);
            entity.Property(e => e.SupplierPhoneNumber).HasMaxLength(20);
            entity.Property(e => e.TaxId)
                .HasMaxLength(50)
                .HasColumnName("TaxID");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.VoucherId).HasName("PK__Vouchers__3AEE79C1074D2BB0");

            entity.HasIndex(e => e.Code, "UQ__Vouchers__A25C5AA7B0D62728").IsUnique();

            entity.Property(e => e.VoucherId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("VoucherID");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.ExpiredDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxDiscountAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MinOrderAmount).HasColumnType("decimal(10, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
