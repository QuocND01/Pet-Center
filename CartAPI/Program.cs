using AutoMapper;
using CartAPI.Mappings;
using CartAPI.Models;
using CartAPI.Repositories;
using CartAPI.Repositories.Interfaces;
using CartAPI.Services;
using CartAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// 1. DbContext
builder.Services.AddDbContext<PetCenterCartServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Repositories
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartDetailRepository, CartDetailRepository>();

// 3. Services
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ProductApiService>();

// 4. AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// 5. HttpClient for ProductAPI
builder.Services.AddHttpClient("ProductAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProductAPI"]
        ?? throw new InvalidOperationException("Services:ProductAPI not configured"));
});

// 6. JWT Authentication (cùng key với CustomerService / AuthService)
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
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = ctx => { ctx.Response.StatusCode = 401; return Task.CompletedTask; },
        OnForbidden = ctx => { ctx.Response.StatusCode = 403; return Task.CompletedTask; }
    };
});

builder.Services.AddAuthorization();

// 7. Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 8. Swagger with JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CartAPI", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token: Bearer {your JWT token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
        policy.WithOrigins(builder.Configuration["Cors:Origin"] ?? "https://localhost:7010")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Docker"))
    app.UseHttpsRedirection();

app.UseCors("AllowClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();