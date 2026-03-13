using Microsoft.EntityFrameworkCore;
using OrdersAPI.Models;
using OrdersAPI.Repository;
using OrdersAPI.Repository.Interface;
using OrdersAPI.Service;
using OrdersAPI.Service.Interface;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// OrderService uses PetCenter_OrderService DB
builder.Services.AddDbContext<PetCenterOrderServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CartService uses PetCenter_CartService DB (separate DB)
builder.Services.AddDbContext<PetCenterCartServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CartConnection")));

// Order services
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();

// Cart services
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();

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