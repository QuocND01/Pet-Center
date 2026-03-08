using PetCenterClient.Services;
using PetCenterClient.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var apiUrl = builder.Configuration["Api:url"];

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

builder.Services.AddSession();

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
