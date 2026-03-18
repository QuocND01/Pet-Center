using PetCenterClient.DTOs;
using PetCenterClient.Services;
using PetCenterClient.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

var apiUrl = builder.Configuration["Api:url"];

builder.Services.AddHttpClient<IStaffService, StaffService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IProductService, ProductService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IBrandService, BrandService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<ICategoryService, CategoryService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<ICustomerService, CustomerService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IImportStockService, ImportStockService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<ISupplierService, SupplierService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IAddressServiceClient, AddressServiceClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IOrderDetailServiceClient, OrderDetailServiceClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<ICartService, CartService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

// ✅ Register CheckoutService
builder.Services.AddHttpClient<ICheckoutService, CheckoutService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});
builder.Services.AddHttpClient<IStatisticsServiceClient, StatisticsServiceClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.Configure<GoogleClientDto>(
    builder.Configuration.GetSection("Authentication:Google"));

builder.Services.AddScoped<IGoogleClientService, GoogleClientService>();

builder.Services.AddSession();
builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();