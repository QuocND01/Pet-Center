using PetCenterClient.Common;
using PetCenterClient.DTOs;
using PetCenterClient.Services;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.Login;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                 optional: true,
                 reloadOnChange: true);


var apiUrl = builder.Configuration["Api:url"];
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<IBrandAPIClient, BrandAPIClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IProductAPIClient, ProductAPIClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<ICategoryAPIClient, CategoryAPIClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});


builder.Services.AddHttpClient<IServiceAPIClient, ServiceAPIClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IStaffService, StaffService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<ICustomerApiService, CustomerApiService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IImportStockService, ImportStockService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<ISupplierApiService, SupplierApiService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IAddressAPIClient, AddressAPIClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IAddressServiceClient, AddressServiceClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IOrderAPIClient, OrderAPIClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});


builder.Services.AddHttpClient<ICartService, CartService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IFeedbackApiService, FeedbackApiService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IVoucherApiService, VoucherApiService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IFeedbackApiService, FeedbackApiService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});
builder.Services.AddHttpClient<InventoryApiService>();



// ✅ Register CheckoutService.
// ✅ Register CheckoutServiceABC
builder.Services.AddHttpClient<ICheckoutService, CheckoutService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});
builder.Services.AddHttpClient<IStatisticsServiceClient, StatisticsServiceClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});
builder.Services.AddHttpClient<IAdminFeedbackApiService, AdminFeedbackApiService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IMedicalRecordAPIClient, MedicalRecordAPIClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IPrescriptionItemAPIClient, PrescriptionItemAPIClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});
//Excel service
builder.Services.AddScoped<ExcelService>();


builder.Services.Configure<GoogleClientViewModel>(
    builder.Configuration.GetSection("Authentication:Google"));

builder.Services.AddScoped<IGoogleAPIClient, GoogleAPIClient>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // quan trọng cho Google OAuth popup
});
builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ApiExceptionFilter>();
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsEnvironment("Docker")) { app.UseHttpsRedirection(); }
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();