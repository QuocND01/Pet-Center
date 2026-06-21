using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Common;

namespace PetCenterAPI.Models;

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

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentService> AppointmentServices { get; set; }

    public virtual DbSet<AppointmentSnapshot> AppointmentSnapshots { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartDetail> CartDetails { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryAttribute> CategoryAttributes { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerVoucher> CustomerVouchers { get; set; }

    public virtual DbSet<FeedbackImage> FeedbackImages { get; set; }

    public virtual DbSet<ImportProductSnapshot> ImportProductSnapshots { get; set; }

    public virtual DbSet<ImportStock> ImportStocks { get; set; }

    public virtual DbSet<ImportStockDetail> ImportStockDetails { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<OrderProductSnapshot> OrderProductSnapshots { get; set; }

    public virtual DbSet<OtpCode> OtpCodes { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Pet> Pets { get; set; }

    public virtual DbSet<PrescriptionItem> PrescriptionItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductAttribute> ProductAttributes { get; set; }

    public virtual DbSet<ProductFeedback> ProductFeedbacks { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceImage> ServiceImages { get; set; }

    public virtual DbSet<Staff> Staffs { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<VetFeedback> VetFeedbacks { get; set; }

    public virtual DbSet<VetProfile> VetProfiles { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=127.0.0.1,1433;Database=PetCenter;User Id=sa;Password=123456;TrustServerCertificate=True;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__Addresse__091C2A1BC1094C55");

            entity.Property(e => e.AddressId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("AddressID");
            entity.Property(e => e.AddressDetails)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.District)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.Province)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Ward)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Customer).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Addresses_Customers");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCA21AFC4A4C");

            entity.Property(e => e.AppointmentId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("AppointmentID");
            entity.Property(e => e.AppointmentEnd).HasColumnType("datetime");
            entity.Property(e => e.AppointmentStart).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Note)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PetId).HasColumnName("PetID");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Customer).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointments_Customers");

            entity.HasOne(d => d.Pet).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointments_Pets");

            entity.HasOne(d => d.Staff).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointments_Staffs");
        });

        modelBuilder.Entity<AppointmentService>(entity =>
        {
            entity.HasKey(e => e.AppointmentServiceId).HasName("PK__Appointm__3B38F276F6F3E84E");

            entity.Property(e => e.AppointmentServiceId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("AppointmentServiceID");
            entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
            entity.Property(e => e.Duration)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.PriceAtBooking).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ServiceType).HasDefaultValue(1);

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentServices)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppointmentServices_Appointments");

            entity.HasOne(d => d.Service).WithMany(p => p.AppointmentServices)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppointmentServices_Services");
        });

        modelBuilder.Entity<AppointmentSnapshot>(entity =>
        {
            entity.HasKey(e => e.AppointmentSnapshotId).HasName("PK__Appointm__93064B14BCBDD04E");

            entity.ToTable("AppointmentSnapshot");

            entity.HasIndex(e => e.AppointmentId, "UQ__Appointm__8ECDFCA3E5DD6B73").IsUnique();

            entity.Property(e => e.AppointmentSnapshotId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("AppointmentSnapshotID");
            entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
            entity.Property(e => e.Breed)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ExperienceYears).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Gender)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Rating).HasColumnType("decimal(2, 1)");
            entity.Property(e => e.Species)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.VetName).HasMaxLength(100);
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Appointment).WithOne(p => p.AppointmentSnapshot)
                .HasForeignKey<AppointmentSnapshot>(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppointmentSnapshot_Appointments");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__Brands__DAD4F3BEEF9EA9BB");

            entity.HasIndex(e => e.BrandName, "UQ__Brands__2206CE9B8900F156").IsUnique();

            entity.Property(e => e.BrandId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("BrandID");
            entity.Property(e => e.BrandDescription)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.BrandLogo).HasMaxLength(255);
            entity.Property(e => e.BrandName).HasMaxLength(150);
            entity.Property(e => e.PublicId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
.HasConversion<int>()
.HasDefaultValue(Status.Active);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Cart__51BCD79700BC6423");

            entity.ToTable("Cart");

            entity.HasIndex(e => e.CustomerId, "UQ__Cart__A4AE64B9234CAA91").IsUnique();

            entity.Property(e => e.CartId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("CartID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

            entity.HasOne(d => d.Customer).WithOne(p => p.Cart)
                .HasForeignKey<Cart>(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cart_Customers");
        });

        modelBuilder.Entity<CartDetail>(entity =>
        {
            entity.HasKey(e => e.CartDetailsId).HasName("PK__CartDeta__88F1342189E18C32");

            entity.Property(e => e.CartDetailsId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("CartDetailsID");
            entity.Property(e => e.CartId).HasColumnName("CartID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartDetails)
                .HasForeignKey(d => d.CartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CartDetails_Cart");

            entity.HasOne(d => d.Product).WithMany(p => p.CartDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CartDetails_Products");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2BA01CB17D");

            entity.HasIndex(e => e.CategoryName, "UQ__Categori__8517B2E0886AD229").IsUnique();

            entity.Property(e => e.CategoryId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("CategoryID");
            entity.Property(e => e.CategoryDescription)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.CategoryLogo).HasMaxLength(255);
            entity.Property(e => e.CategoryName).HasMaxLength(150);
            entity.Property(e => e.PublicId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
    .HasConversion<int>()
    .HasDefaultValue(Status.Active);
        });

        modelBuilder.Entity<CategoryAttribute>(entity =>
        {
            entity.HasKey(e => e.CategoryAttributeId).HasName("PK__Category__21D38D92A3E18DE8");

            entity.HasIndex(e => new { e.CategoryId, e.AttributeName }, "UQ_CategoryAttributes").IsUnique();

            entity.Property(e => e.CategoryAttributeId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("CategoryAttributeID");
            entity.Property(e => e.AttributeName).HasMaxLength(150);
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryAttributes)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CategoryAttributes_Categories");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B82091B8A0");

            entity.HasIndex(e => e.Email, "UQ__Customer__A9D1053451343801").IsUnique();

            entity.Property(e => e.CustomerId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("CustomerID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsVerified).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<CustomerVoucher>(entity =>
        {
            entity.HasKey(e => new { e.CustomerId, e.VoucherId }).HasName("PK__Customer__A7008324A519F6CD");

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.VoucherId).HasColumnName("VoucherID");
            entity.Property(e => e.IsUsed).HasDefaultValue(false);

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerVouchers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerVouchers_Customers");

            entity.HasOne(d => d.Voucher).WithMany(p => p.CustomerVouchers)
                .HasForeignKey(d => d.VoucherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerVouchers_Vouchers");
        });

        modelBuilder.Entity<FeedbackImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__Feedback__7516F4EC721466A5");

            entity.Property(e => e.ImageId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ImageID");
            entity.Property(e => e.FeedbackId).HasColumnName("FeedbackID");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PublicId).HasMaxLength(50);

            entity.HasOne(d => d.Feedback).WithMany(p => p.FeedbackImages)
                .HasForeignKey(d => d.FeedbackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Images_ProductFeedbacks");
        });

        modelBuilder.Entity<ImportProductSnapshot>(entity =>
        {
            entity.HasKey(e => e.ProductSnapshotId).HasName("PK__ImportPr__AB58F7985A89209B");

            entity.ToTable("ImportProductSnapshot");

            entity.HasIndex(e => e.ImportStockDetailsId, "UQ__ImportPr__3EE53B5B6F34C12F").IsUnique();

            entity.Property(e => e.ProductSnapshotId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ProductSnapshotID");
            entity.Property(e => e.ImportStockDetailsId).HasColumnName("ImportStockDetailsID");
            entity.Property(e => e.ProductBrand).HasMaxLength(100);
            entity.Property(e => e.ProductCategory).HasMaxLength(100);
            entity.Property(e => e.ProductImage).HasMaxLength(255);
            entity.Property(e => e.ProductName).HasMaxLength(255);

            entity.HasOne(d => d.ImportStockDetails).WithOne(p => p.ImportProductSnapshot)
                .HasForeignKey<ImportProductSnapshot>(d => d.ImportStockDetailsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImportProductSnapshot_Details");
        });

        modelBuilder.Entity<ImportStock>(entity =>
        {
            entity.HasKey(e => e.ImportId).HasName("PK__ImportSt__8697678A3BDF1971");

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
            entity.Property(e => e.Status).HasDefaultValue(ImportStock.ImportStatus.Pending);
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Staff).WithMany(p => p.ImportStocks)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImportStocks_Staffs");

            entity.HasOne(d => d.Supplier).WithMany(p => p.ImportStocks)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImportStocks_Suppliers");
        });

        modelBuilder.Entity<ImportStockDetail>(entity =>
        {
            entity.HasKey(e => e.ImportStockDetailsId).HasName("PK__ImportSt__3EE53B5A7C2F3A90");

            entity.Property(e => e.ImportStockDetailsId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ImportStockDetailsID");
            entity.Property(e => e.ImportId).HasColumnName("ImportID");
            entity.Property(e => e.ImportPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.SKU).HasMaxLength(100);
            entity.Property(e => e.BatchCode).HasMaxLength(100);

            entity.HasOne(d => d.Import).WithMany(p => p.ImportStockDetails)
                .HasForeignKey(d => d.ImportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImportStockDetails_ImportStocks");

            entity.HasOne(d => d.Product).WithMany(p => p.ImportStockDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImportStockDetails_Products");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("PK__Inventor__F5FDE6D30297BC7F");

            entity.ToTable("Inventory");

            entity.HasIndex(e => e.ProductId, "UQ__Inventor__B40CC6EC9CEA5150").IsUnique();

            entity.Property(e => e.InventoryId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("InventoryID");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.SKU).HasMaxLength(100);
            entity.HasOne(d => d.Product).WithOne(p => p.Inventory)
                .HasForeignKey<Inventory>(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inventory_Products");
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Inventor__55433A4BDA4D74B5");

            entity.Property(e => e.TransactionId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("TransactionID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.InventoryId).HasColumnName("InventoryID");
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.ReferenceId).HasColumnName("ReferenceID");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Inventory).WithMany(p => p.InventoryTransactions)
                .HasForeignKey(d => d.InventoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryTransactions_Inventory");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__MedicalR__FBDF78C9F5DE9996");

            entity.Property(e => e.RecordId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("RecordID");
            entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Diagnosis)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.Treatment)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.Appointment).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalRecords_Appointments");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAFED6968E2");

            entity.Property(e => e.OrderId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("OrderID");
            entity.Property(e => e.AddressId).HasColumnName("AddressID");
            entity.Property(e => e.AddressSnapshot)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.DeliveredDate).HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("COD");
            entity.Property(e => e.PaymentStatus).HasDefaultValue(1);
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");

            entity.HasOne(d => d.Address).WithMany(p => p.Orders)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Addresses");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Customers");

            entity.HasOne(d => d.Staff).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK_Orders_Staffs");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailsId).HasName("PK__OrderDet__9DD74D9D9C90F2B4");

            entity.Property(e => e.OrderDetailsId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("OrderDetailsID");
            entity.Property(e => e.ImportStockDetailsId).HasColumnName("ImportStockDetailsID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.ImportStockDetails).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ImportStockDetailsId)
                .HasConstraintName("FK_OrderDetails_ImportStockDetails");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDetails_Orders");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDetails_Products");
        });

        modelBuilder.Entity<OrderProductSnapshot>(entity =>
        {
            entity.HasKey(e => e.ProductSnapshotId).HasName("PK__OrderPro__AB58F798E1600109");

            entity.ToTable("OrderProductSnapshot");

            entity.HasIndex(e => e.OrderDetailsId, "UQ__OrderPro__9DD74D9CDB6B5A36").IsUnique();

            entity.Property(e => e.ProductSnapshotId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ProductSnapshotID");
            entity.Property(e => e.OrderDetailsId).HasColumnName("OrderDetailsID");
            entity.Property(e => e.ProductBrand).HasMaxLength(255);
            entity.Property(e => e.ProductCategory).HasMaxLength(255);
            entity.Property(e => e.ProductDescription).HasMaxLength(255);
            entity.Property(e => e.ProductImage).HasMaxLength(255);
            entity.Property(e => e.ProductName).HasMaxLength(255);
            entity.Property(e => e.ProductPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.OrderDetails).WithOne(p => p.OrderProductSnapshot)
                .HasForeignKey<OrderProductSnapshot>(d => d.OrderDetailsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderProductSnapshot_OrderDetails");
        });

        modelBuilder.Entity<OtpCode>(entity =>
        {
            entity.HasKey(e => e.OtpId).HasName("PK__OtpCodes__3143C483F089BC48");

            entity.Property(e => e.OtpId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("OtpID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.LastOtpSentAt)
                .HasColumnType("datetime")
                .HasColumnName("LastOtpSentAT");
            entity.Property(e => e.VerificationCode).HasMaxLength(6);
            entity.Property(e => e.VerificationExpire).HasColumnType("datetime");

            entity.HasOne(d => d.Customer).WithMany(p => p.OtpCodes)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OtpCodes_Customers");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A58A2FC7EF2");

            entity.Property(e => e.PaymentId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("PaymentID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BankCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.GatewayTransactionNo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaidAt).HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ResponseCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.TransactionRef)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Orders");
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.PetId).HasName("PK__Pets__48E53802D06E03C0");

            entity.Property(e => e.PetId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("PetID");
            entity.Property(e => e.Breed)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Gender)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Note)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PetAvatar)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PublicId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Species)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Customer).WithMany(p => p.Pets)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pets_Customers");
        });

        modelBuilder.Entity<PrescriptionItem>(entity =>
        {
            entity.HasKey(e => e.PrescriptionItemId).HasName("PK__Prescrip__1AADD9DA4FD26B3F");

            entity.Property(e => e.PrescriptionItemId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("PrescriptionItemID");
            entity.Property(e => e.Dosage)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Duration)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.MedicineName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Note)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.Status).HasDefaultValue(1);

            entity.HasOne(d => d.Record).WithMany(p => p.PrescriptionItems)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrescriptionItems_MedicalRecords");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6ED966A0BF3");

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
            entity.Property(e => e.Status)
.HasConversion<int>()
.HasDefaultValue(Status.Active);
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Brands");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Categories");
        });

        modelBuilder.Entity<ProductAttribute>(entity =>
        {
            entity.HasKey(e => e.ProductAttributesId).HasName("PK__ProductA__596DE210C4AC8069");

            entity.Property(e => e.ProductAttributesId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ProductAttributesID");
            entity.Property(e => e.AttributeValue).HasMaxLength(255);
            entity.Property(e => e.CategoryAttributeId).HasColumnName("CategoryAttributeID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.CategoryAttribute).WithMany(p => p.ProductAttributes)
                .HasForeignKey(d => d.CategoryAttributeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductAttributes_CategoryAttributes");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductAttributes)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductAttributes_Products");
        });

        modelBuilder.Entity<ProductFeedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__ProductF__6A4BEDF66D793DE0");

            entity.Property(e => e.FeedbackId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("FeedbackID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.ReplyDate).HasColumnType("datetime");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Customer).WithMany(p => p.ProductFeedbacks)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductFeedbacks_Customers");

            entity.HasOne(d => d.Order).WithMany(p => p.ProductFeedbacks)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductFeedbacks_Orders");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductFeedbacks)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductFeedbacks_Products");

            entity.HasOne(d => d.Staff).WithMany(p => p.ProductFeedbacks)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK_ProductFeedbacks_Staffs");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__ProductI__7516F4EC3D4EB488");

            entity.Property(e => e.ImageId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ImageID");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.PublicId).HasMaxLength(50);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Images_Products");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3A40D7995D");

            entity.Property(e => e.RoleId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("RoleID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Services__C51BB0EA51E4E238");

            entity.Property(e => e.ServiceId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ServiceID");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ServiceType).HasDefaultValue(1);
            entity.Property(e => e.Status)
.HasConversion<int>()
.HasDefaultValue(Status.Active);
        });

        modelBuilder.Entity<ServiceImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__ServiceI__7516F4EC02E99E6B");

            entity.Property(e => e.ImageId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ImageID");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PublicId).HasMaxLength(50);
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");

            entity.HasOne(d => d.Service).WithMany(p => p.ServiceImages)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Images_Services");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PK__Staffs__96D4AAF72EE69CDC");

            entity.HasIndex(e => e.Email, "UQ__Staffs__A9D105344FCCEE5B").IsUnique();

            entity.Property(e => e.StaffId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("StaffID");
            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.BirthDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Gender)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.HireDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.PublicId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasMany(d => d.Roles).WithMany(p => p.Staff)
                .UsingEntity<Dictionary<string, object>>(
                    "StaffRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_StaffRoles_Roles"),
                    l => l.HasOne<Staff>().WithMany()
                        .HasForeignKey("StaffId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_StaffRoles_Staffs"),
                    j =>
                    {
                        j.HasKey("StaffId", "RoleId").HasName("PK__StaffRol__2E7B0614D6A3542F");
                        j.ToTable("StaffRoles");
                        j.IndexerProperty<Guid>("StaffId").HasColumnName("StaffID");
                        j.IndexerProperty<Guid>("RoleId").HasColumnName("RoleID");
                    });
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE6669473A9D7C5");

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

        modelBuilder.Entity<VetFeedback>(entity =>
        {
            entity.HasKey(e => e.VetFeedbackId).HasName("PK__VetFeedb__C95D002CE8B12979");

            entity.Property(e => e.VetFeedbackId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("VetFeedbackID");
            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Customer).WithMany(p => p.VetFeedbacks)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VetFeedbacks_Customers");

            entity.HasOne(d => d.Staff).WithMany(p => p.VetFeedbacks)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VetFeedbacks_Staffs");
        });

        modelBuilder.Entity<VetProfile>(entity =>
        {
            entity.HasKey(e => e.VetProfileId).HasName("PK__VetProfi__CEF0EDC122C46477");

            entity.HasIndex(e => e.StaffId, "UQ__VetProfi__96D4AAF654ADDC3C").IsUnique();

            entity.HasIndex(e => e.LicenseNumber, "UQ__VetProfi__E88901664B701FAA").IsUnique();

            entity.Property(e => e.VetProfileId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("VetProfileID");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ExperienceYears).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LicenseNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StaffId).HasColumnName("StaffID");

            entity.HasOne(d => d.Staff).WithOne(p => p.VetProfile)
                .HasForeignKey<VetProfile>(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VetProfiles_Staffs");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.VoucherId).HasName("PK__Vouchers__3AEE79C115689580");

            entity.HasIndex(e => e.Code, "UQ__Vouchers__A25C5AA722A86399").IsUnique();

            entity.Property(e => e.VoucherId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("VoucherID");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.ExpiredDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxDiscountAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MinOrderAmount).HasColumnType("decimal(18, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
