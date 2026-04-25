using InventoryAPI.Models;
using InventoryAPI.Profiles;
using InventoryAPI.Repository;
using InventoryAPI.Repository.Interface;
using InventoryAPI.Service;
using InventoryAPI.Service.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                 optional: true,
                 reloadOnChange: true);


// Add services to the container.

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

    // Giống JwtEntryPoint + AccessDenied
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
        }
    };
});



builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductAPI", Version = "v1" });

    // Cấu hình hỗ trợ Bearer token
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
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddControllers();
builder.Services.AddAuthorization();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<PetCenterInventoryServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbConnection")));


// Đăng ký Automapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<InventoryProfile>());


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient",
        policy =>
        {
            policy.WithOrigins("https://localhost:7010")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


// Đăng ký Service và Repository
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Docker")) { app.UseHttpsRedirection(); }

app.UseCors("AllowClient");


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
