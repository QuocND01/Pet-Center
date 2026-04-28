using AttendanceAPI.Models; // Thay bằng namespace chứa PetCenterAttendanceContext của bạn
using AttendanceAPI.Repository;
using AttendanceAPI.Repository.Interface;
using AttendanceAPI.Service;
using AttendanceAPI.Service.Interface;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// tự load theo ENV
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                 optional: true,
                 reloadOnChange: true);

// 1. Cấu hình DbContext (Kết nối Database)
// Nhớ đảm bảo trong appsettings.json của bạn có chuỗi kết nối tên là "DefaultConnection"
builder.Services.AddDbContext<PetCenterAttendanceServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình AutoMapper
// Quét toàn bộ Assembly hiện tại để tìm các class kế thừa từ Profile (như MappingProfile của bạn)
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// 3. Đăng ký Dependency Injection (DI) cho Repository và Service
// Dùng AddScoped vì vòng đời của chúng nên gắn liền với mỗi HTTP Request
builder.Services.AddScoped<IStaffShiftRepository, StaffShiftRepository>();
builder.Services.AddScoped<IStaffShiftService, StaffShiftService>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IShiftTemplateRepository, ShiftTemplateRepository>();
builder.Services.AddScoped<IShiftTemplateService, ShiftTemplateService>();
// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// (Tùy chọn) Bật CORS nếu frontend (ví dụ: React, Flutter) gọi API từ domain khác
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

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

// Bật CORS đã config ở trên
app.UseCors("AllowAll");

// Khai báo xác thực/phân quyền (nếu bạn dùng JWT Token thì app.UseAuthentication() phải đứng trước)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();