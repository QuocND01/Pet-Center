using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using PetCenterAPI.Service.Interface;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                 optional: true,
                 reloadOnChange: true);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Database Contexts ─────────────────────────────────────────────
builder.Services.AddDbContext<PetCenterOrderServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// ── Repositories ─────────────────────────────────────────────────
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();


// ── Services ─────────────────────────────────────────────────────
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();

// ── AutoMapper ────────────────────────────────────────────────────
// MappingProfile là class có sẵn trong OrdersAPI/Profile/MappingProfile.cs
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Docker")) { app.UseHttpsRedirection(); }
app.UseAuthorization();
app.MapControllers();
app.Run();