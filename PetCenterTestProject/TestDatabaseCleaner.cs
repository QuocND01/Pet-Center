using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;

namespace PetCenterTestProject;

internal static class TestDatabaseCleaner
{
    public static async Task ClearAllAsync(PetCenterContext context)
    {
        context.ChangeTracker.Clear();

        await context.Database.ExecuteSqlRawAsync("DELETE FROM [StaffRoles];");

        context.PrescriptionItems.RemoveRange(context.PrescriptionItems);
        context.MedicalRecords.RemoveRange(context.MedicalRecords);
        context.AppointmentSnapshots.RemoveRange(context.AppointmentSnapshots);
        context.AppointmentServices.RemoveRange(context.AppointmentServices);
        context.Appointments.RemoveRange(context.Appointments);
        context.VetFeedbacks.RemoveRange(context.VetFeedbacks);
        context.VetProfiles.RemoveRange(context.VetProfiles);
        context.ScheduleExceptions.RemoveRange(context.ScheduleExceptions);
        context.GlobalWorkSchedules.RemoveRange(context.GlobalWorkSchedules);
        context.CartDetails.RemoveRange(context.CartDetails);
        context.Carts.RemoveRange(context.Carts);
        context.CustomerVouchers.RemoveRange(context.CustomerVouchers);
        context.Vouchers.RemoveRange(context.Vouchers);
        context.FeedbackImages.RemoveRange(context.FeedbackImages);
        context.ProductFeedbacks.RemoveRange(context.ProductFeedbacks);
        context.OrderProductSnapshots.RemoveRange(context.OrderProductSnapshots);
        context.OrderDetails.RemoveRange(context.OrderDetails);
        context.Payments.RemoveRange(context.Payments);
        context.Orders.RemoveRange(context.Orders);
        context.OtpCodes.RemoveRange(context.OtpCodes);
        context.Addresses.RemoveRange(context.Addresses);
        context.Pets.RemoveRange(context.Pets);
        context.Customers.RemoveRange(context.Customers);
        context.InventoryTransactions.RemoveRange(context.InventoryTransactions);
        context.ImportProductSnapshots.RemoveRange(context.ImportProductSnapshots);
        context.ImportStockDetails.RemoveRange(context.ImportStockDetails);
        context.ImportStocks.RemoveRange(context.ImportStocks);
        context.Suppliers.RemoveRange(context.Suppliers);
        context.Staffs.RemoveRange(context.Staffs);
        context.Roles.RemoveRange(context.Roles);
        context.ProductAttributes.RemoveRange(context.ProductAttributes);
        context.ProductImages.RemoveRange(context.ProductImages);
        context.Inventories.RemoveRange(context.Inventories);
        context.Products.RemoveRange(context.Products);
        context.CategoryAttributes.RemoveRange(context.CategoryAttributes);
        context.Categories.RemoveRange(context.Categories);
        context.Brands.RemoveRange(context.Brands);
        context.ServiceImages.RemoveRange(context.ServiceImages);
        context.Services.RemoveRange(context.Services);
        context.Diseases.RemoveRange(context.Diseases);

        await context.SaveChangesAsync();
    }

    public static async Task ClearCatalogAsync(PetCenterContext context)
    {
        context.FeedbackImages.RemoveRange(context.FeedbackImages);
        context.ProductFeedbacks.RemoveRange(context.ProductFeedbacks);
        context.OrderProductSnapshots.RemoveRange(context.OrderProductSnapshots);
        context.OrderDetails.RemoveRange(context.OrderDetails);
        context.CartDetails.RemoveRange(context.CartDetails);
        context.InventoryTransactions.RemoveRange(context.InventoryTransactions);
        context.ImportProductSnapshots.RemoveRange(context.ImportProductSnapshots);
        context.ImportStockDetails.RemoveRange(context.ImportStockDetails);
        context.ProductAttributes.RemoveRange(context.ProductAttributes);
        context.ProductImages.RemoveRange(context.ProductImages);
        context.Inventories.RemoveRange(context.Inventories);

        await context.SaveChangesAsync();

        context.Products.RemoveRange(context.Products);
        await context.SaveChangesAsync();

        context.CategoryAttributes.RemoveRange(context.CategoryAttributes);
        await context.SaveChangesAsync();

        context.Categories.RemoveRange(context.Categories);
        context.Brands.RemoveRange(context.Brands);
        await context.SaveChangesAsync();
    }

    public static async Task ClearServicesAsync(PetCenterContext context)
    {
        context.ServiceImages.RemoveRange(context.ServiceImages);
        context.AppointmentServices.RemoveRange(context.AppointmentServices);

        await context.SaveChangesAsync();

        context.Services.RemoveRange(context.Services);
        await context.SaveChangesAsync();
    }
}
