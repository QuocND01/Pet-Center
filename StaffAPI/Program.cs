<<<<<<< HEAD
var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                 optional: true,
                 reloadOnChange: true);

=======
﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using StaffAPI.Mappings;
using StaffAPI.Models;
using StaffAPI.Repositories;
using StaffAPI.Repositories.Interfaces;
using StaffAPI.Security;
using StaffAPI.Services;
using StaffAPI.Services.Interfaces;
using System.Text;
>>>>>>> origin/main

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                 optional: true,
                 reloadOnChange: true);

// ── 1. DbContext ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<PetCenterStaffServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── 2. Repositories ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IVetProfileRepository, VetProfileRepository>();
builder.Services.AddScoped<IStaffAuthRepository, StaffAuthRepository>();

// ── 3. Services ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IVetProfileService, VetProfileService>();
builder.Services.AddScoped<IStaffAuthService, StaffAuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddSingleton<PasswordService>();

// ── 4. AutoMapper ─────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ── 5. OData ──────────────────────────────────────────────────────────────────
var odataBuilder = new ODataConventionModelBuilder();
odataBuilder.EntitySet<Staff>("Staffs");
odataBuilder.EntitySet<VetProfile>("VetProfiles");
// Add services to the container.

// File: Program.cs

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    })
    .AddOData(opt => opt
        .Select().Filter().OrderBy().Expand().Count().SetMaxTop(100)
        .AddRouteComponents("odata", odataBuilder.GetEdmModel()));

// ── 6. JWT Authentication ─────────────────────────────────────────────────────
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();
// ── 7. Swagger ────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PetCenter.StaffService API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {token}"
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

// ── 8. CORS ───────────────────────────────────────────────────────────────────
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

if (!app.Environment.IsEnvironment("Docker")) { app.UseHttpsRedirection(); }

<<<<<<< HEAD
=======
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
>>>>>>> origin/main
app.UseAuthorization();
app.MapControllers();

app.Run();
