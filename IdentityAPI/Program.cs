
using IdentityAPI.Models;
using IdentityAPI.Profiles;
using IdentityAPI.Repository;
using IdentityAPI.Repository.Interface;
using IdentityAPI.Security;
using IdentityAPI.Service;
using IdentityAPI.Service.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using System.Text;

namespace IdentityAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // ===== OData Configuration =====
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Customer>("Customers");
            modelBuilder.EntitySet<Staff>("Staffs");

            // ===== AddControllers with OData =====
            builder.Services
                .AddControllers()
                .AddOData(opt =>
                    opt.AddRouteComponents("odata", modelBuilder.GetEdmModel())
                       .Select()
                       .Expand()
                       .Filter()
                       .OrderBy()
                       .SetMaxTop(100)
                       .Count());

            // ===== AutoMapper Configuration =====
            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<CustomerMappingProfile>());
            builder.Services.AddScoped<IJwtService, JwtService>();

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

            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<ICustomerAuthService, CustomerAuthService>();

            builder.Services.AddScoped<IStaffRepository, StaffRepository>();
            builder.Services.AddScoped<IStaffService, StaffService>();
            builder.Services.AddScoped<IStaffAuthService, StaffAuthService>();
            builder.Services.AddScoped<PasswordService>();

            builder.Services.AddScoped<ICustomerService, CustomerService>();

            builder.Services.AddScoped<IEmailService, EmailService>();

            builder.Services.Configure<GoogleAuthSettings>(
    builder.Configuration.GetSection("Authentication:Google"));
            builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            builder.Services.AddHttpClient();

            builder.Services.AddScoped<IForgotPasswordService, ForgotPasswordService>();

            builder.Services.AddAuthorization();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            builder.Services.AddDbContext<PetCenterIdentityServiceDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbConnection")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


            app.MapControllers();

            app.Run();
        }
    }
}
