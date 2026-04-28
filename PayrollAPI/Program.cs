using Microsoft.EntityFrameworkCore;
using PayrollAPI.Models; // Đảm bảo namespace này khớp với nơi chứa PetCenterPayrollServiceContext
using PayrollAPI.Repository;
using PayrollAPI.Repository.Interface;
using PayrollAPI.Service;
using PayrollAPI.Service.Interface;

var builder = WebApplication.CreateBuilder(args);

// Tự load config theo Environment
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                 optional: true,
                 reloadOnChange: true);

// ==========================================
// 1. CẤU HÌNH DATABASE (DbContext)
// ==========================================
builder.Services.AddDbContext<PetCenterPayrollServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ==========================================
// 2. CẤU HÌNH AUTOMAPPER
// ==========================================
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ==========================================
// 3. ĐĂNG KÝ DEPENDENCY INJECTION (DI)
// ==========================================
builder.Services.AddScoped<IViolationRepository, ViolationRepository>();
builder.Services.AddScoped<IViolationService, ViolationService>();

// Add services to the container.
builder.Services.AddControllers();

// Bật CORS để cho phép Frontend (React, Flutter...) gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

// Gọi middleware CORS trước Authorization
app.UseCors("AllowAll");

// Nếu sau này bạn có phân quyền JWT, hãy uncomment dòng dưới đây và để nó TRƯỚC app.UseAuthorization()
// app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();