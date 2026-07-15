using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PetCenterAPI.Hubs; 
using PetCenterAPI.Models;
using PetCenterAPI.Odata;
using PetCenterAPI.Profiles;
using PetCenterAPI.Repositories;
using PetCenterAPI.Repositories.Interfaces;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Security;
using PetCenterAPI.Service;
using PetCenterAPI.Service.Interface;
using PetCenterAPI.Services;
using PetCenterAPI.Services.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                 optional: true,
                 reloadOnChange: true);

// ── ĐĂNG KÝ SIGNALR ───────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── CẤU HÌNH AUTHENTICATION & JWT ─────────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        )
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        },
        // BẮT BUỘC CHO SIGNALR: Đọc token từ query string
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/appHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddHostedService<CleanupProductImageJob>();

// ── CẤU HÌNH SWAGGER ─────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductAPI", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token theo dạng: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    // ĐÃ FIX LỖI AMBIGUOUS REFERENCE Ở ĐÂY
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

// ── CẤU HÌNH DATABASE VÀ ODATA ───────────────────────────────────────────────
builder.Services.AddDbContext<PetCenterContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbConnection"))
    .EnableSensitiveDataLogging()
    .LogTo(Console.WriteLine, LogLevel.Information))
    ;

builder.Services
    .AddControllers()
    .AddOData(opt => opt
        .AddRouteComponents(
            "odata",
            EdmModelBuilder.GetEdmModel()
        )
        .Select()
        .Filter()
        .OrderBy()
        .Expand()
        .Count()
        .SetMaxTop(100)
    );

// ── CẤU HÌNH AUTOMAPPER ───────────────────────────────────────────────────────
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

// ── CẤU HÌNH CORS CHO SIGNALR VÀ RASA (GỘP CHUNG) ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7010", // URL của Client MVC
                "http://localhost:5005",  // URL của Rasa
                "http://localhost:5055"   // URL của Rasa Action Server
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // <-- Bắt buộc phải có cho SignalR
    });
});

// ── ĐĂNG KÝ SERVICES & REPOSITORIES ──────────────────────────────────────────
// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IImportStockRepository, ImportStockRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IStaffAuthRepository, StaffAuthRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IAdminFeedbackRepository, AdminFeedbackRepository>();
builder.Services.AddScoped<IProductFeedbackRepository, ProductFeedbackRepository>();
builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
builder.Services.AddScoped<IPrescriptionItemRepository, PrescriptionItemRepository>();
builder.Services.AddScoped<IChatbotRepository, ChatbotRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IDiseaseRepository, DiseaseRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
builder.Services.AddScoped<IClassifyAIRepository, ClassifyAIRepository>();


// Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICustomerAuthService, CustomerAuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddScoped<IStaffAuthService, StaffAuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAdminFeedbackService, AdminFeedbackService>();
builder.Services.AddScoped<IProductFeedbackService, ProductFeedbackService>();
builder.Services.AddScoped<IImportStockService, ImportStockService>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
builder.Services.AddScoped<IPrescriptionItemService, PrescriptionItemService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IChatbotService, ChatbotService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IAppointmentService,PetCenterAPI.Service.AppointmentService>();
builder.Services.AddScoped<IDiseaseService, DiseaseService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IClassifyAIService, ClassifyAIService>();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("Authentication:Google"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddHttpClient();


builder.Services.AddHttpClient<IClassifyAIRepository, ClassifyAIRepository>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AI:BaseUrl"]);
});

// ── CẤU HÌNH PIPELINE ─────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Docker") && !app.Environment.IsDevelopment()) { app.UseHttpsRedirection(); }

app.UseCors("AllowAll"); // Chỉ để lại 1 dòng này
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ĐĂNG KÝ ENDPOINT CHO HUB CỦA SIGNALR
app.MapHub<AppHub>("/appHub");

app.Run();