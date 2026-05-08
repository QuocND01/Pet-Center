using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using FeedbackAPI.Models;
using FeedbackAPI.Repository.Interface;
using FeedbackAPI.Repository;
using FeedbackAPI.Service;
using FeedbackAPI.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Microsoft.OpenApi.Models;

namespace FeedbackAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
                builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                     optional: true,
                     reloadOnChange: true);

            // Add services to the container.

            builder.Services.AddControllers();

            // ── Swagger + Bearer ──────────────────────────────────────
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FeedbackAPI", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập token: Bearer {your_token}"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
            });

            // ── 1. DbContext ──────────────────────────────────────────────────────────────
            builder.Services.AddDbContext<PetCenterFeedbackServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbConnection")));

            // ── 2. Repositories ──────────────────────────────────────────────────────────
            builder.Services.AddScoped<IProductFeedbackRepository, ProductFeedbackRepository>();
            builder.Services.AddScoped<IAdminFeedbackRepository, AdminFeedbackRepository>();

            // ── 3. Services ───────────────────────────────────────────────────────────────
            builder.Services.AddScoped<IFeedbackService, FeedbackService>();
            builder.Services.AddScoped<IProductFeedbackService, ProductFeedbackService>();
            builder.Services.AddScoped<IAdminFeedbackService, AdminFeedbackService>();
            builder.Services.AddScoped<IEnrichmentService, EnrichmentService>();

            // ── 4. AutoMapper ─────────────────────────────────────────────────────────────
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // ── 5. Http ─────────────────────────────────────────────────────────────
            builder.Services.AddHttpClient("CustomerAPI", c =>
    c.BaseAddress = new Uri(builder.Configuration["InternalServices:CustomerAPI"]!));
            builder.Services.AddHttpClient("ProductAPI", c =>
                c.BaseAddress = new Uri(builder.Configuration["InternalServices:ProductAPI"]!));
            builder.Services.AddHttpClient("StaffAPI", c =>
                c.BaseAddress = new Uri(builder.Configuration["InternalServices:StaffAPI"]!));

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(
                                               builder.Configuration["Jwt:Key"]!))
        };
    });
            builder.Services.AddAuthorization();

            builder.Services.AddCors(options =>
                options.AddPolicy("AllowAll", p =>
                    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

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

            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
