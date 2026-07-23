using PetCenterClient.DependencyInjection;
using PetCenterClient.ViewModels.Login;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                 optional: true,
                 reloadOnChange: true);


var apiUrl = builder.Configuration["Api:url"];
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationHttpClients(apiUrl);

builder.Services.Configure<GoogleClientViewModel>(
    builder.Configuration.GetSection("Authentication:Google"));

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
    //options.Filters.Add<ApiExceptionFilter>();
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