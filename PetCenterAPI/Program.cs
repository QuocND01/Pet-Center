using FluentValidation.AspNetCore;
using PetCenterAPI.Hubs;
using PetCenterAPI.Profiles;
using PetCenterAPI.Service;
using PetCenterAPI.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                 optional: true,
                 reloadOnChange: true);

// ── ĐĂNG KÝ SIGNALR ───────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── CẤU HÌNH HỆ THỐNG DI ──────────────────────
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddHostedService<CleanupProductImageJob>();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDatabaseAndOData(builder.Configuration);
builder.Services.AddCorsConfiguration();
builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<ProductProfile>();
    cfg.AddProfile<BrandProfile>();
    cfg.AddProfile<ServiceProfile>();
    cfg.AddProfile<CategoryProfile>();
    cfg.AddProfile<ProductAttributeProfile>();
    cfg.AddProfile<CategoryAttributeProfile>();
    cfg.AddProfile<CustomerMappingProfile>();
    cfg.AddProfile<SupplierProfile>();
    cfg.AddProfile<OrderProfile>();
    cfg.AddProfile<InventoryProfile>();
    cfg.AddProfile<ImportStockProfile>();
    cfg.AddProfile<AppointmentProfile>();
});

builder.Services.AddHttpClient();

// ── CẤU HÌNH PIPELINE ─────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Docker") && !app.Environment.IsDevelopment()) { app.UseHttpsRedirection(); }

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ĐĂNG KÝ ENDPOINT CHO HUB CỦA SIGNALR
app.MapHub<AppHub>("/appHub");

app.Run();