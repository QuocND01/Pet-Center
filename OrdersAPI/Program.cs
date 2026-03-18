using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrdersAPI.Models;
using OrdersAPI.Repository;
using OrdersAPI.Repository.Interface;
using OrdersAPI.Service;
using OrdersAPI.Service.Interface;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Database Contexts ─────────────────────────────────────────────
builder.Services.AddDbContext<PetCenterOrderServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<PetCenterCartServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CartConnection")));

builder.Services.AddDbContext<PetCenterVoucherServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("VoucherConnection")));

// ── Repositories ─────────────────────────────────────────────────
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();

// ── Services ─────────────────────────────────────────────────────
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();