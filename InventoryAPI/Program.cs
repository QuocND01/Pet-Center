using AutoMapper;
using InventoryAPI.Profiles;
using InventoryAPI.Repository;
using InventoryAPI.Repository.Interface;
using InventoryAPI.Service;
using InventoryAPI.Service.Interface;
using InventoryAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PetCenterContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbConnection")));
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// AutoMapper configuration
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<SupplierProfile>());
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<ImportStockProfile>());
// Dependency Injection
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IImportStockRepository, ImportStockRepository>();
builder.Services.AddScoped<IImportStockService, ImportStockService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
