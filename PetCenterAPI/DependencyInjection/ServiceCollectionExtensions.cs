using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PetCenterAPI.Models;
using PetCenterAPI.Odata;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Security;
using PetCenterAPI.Service;
using PetCenterAPI.Service.Interface;
using System.Text;

namespace PetCenterAPI.DependencyInjection
{
    /// <summary>
    /// Các phương thức mở rộng (Extension Methods) giúp đóng gói đăng ký Dependency Injection của API.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Cấu hình cơ sở dữ liệu Entity Framework Core và dịch vụ OData.
        /// </summary>
        public static IServiceCollection AddDatabaseAndOData(this IServiceCollection services, IConfiguration configuration)
        {
            // Cấu hình Database với Connection String "MyDbConnection" từ appsettings.json
            services.AddDbContext<PetCenterContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("MyDbConnection"))
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information));

            // Đăng ký Controllers kết hợp cấu hình OData định tuyến và truy vấn
            services.AddControllers().AddOData(opt => opt
                .AddRouteComponents("odata", EdmModelBuilder.GetEdmModel())
                .Select()
                .Filter()
                .OrderBy()
                .Expand()
                .Count()
                .SetMaxTop(100)
            );

            return services;
        }

        /// <summary>
        /// Cấu hình xác thực người dùng JWT (JSON Web Token) và hỗ trợ SignalR authentication.
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
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
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
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
                    // BẮT BUỘC CHO SIGNALR: Đọc token từ query string khi kết nối Hub
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

            return services;
        }

        /// <summary>
        /// Cấu hình Swagger hỗ trợ tạo tài liệu API và tích hợp JWT Bearer Token.
        /// </summary>
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductAPI", Version = "v1" });

                // Định nghĩa cơ chế xác thực Bearer Token
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập token theo dạng: Bearer {your JWT token}"
                });

                // Yêu cầu áp dụng cấu hình Security Bearer trên Swagger UI
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }

        /// <summary>
        /// Cấu hình chính sách chia sẻ tài nguyên nguồn gốc chéo (CORS).
        /// </summary>
        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins(
                            "https://localhost:7010", // MVC Client
                            "http://localhost:5005",  // Rasa Chatbot
                            "http://localhost:5055"   // Rasa Action Server
                          )
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // Bắt buộc cho kết nối SignalR
                });
            });

            return services;
        }

        /// <summary>
        /// Tự động quét Assembly hiện tại để đăng ký các Repositories và Services dạng Scoped.
        /// Đồng thời xử lý thủ công các cấu hình đặc biệt (Cloudinary, GoogleAuth, AI API, MoMo, PasswordService).
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Lấy assembly hiện tại chứa code API
            var assembly = typeof(ServiceCollectionExtensions).Assembly;
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
                .ToList();

            // Duyệt qua tất cả các class trong dự án
            foreach (var type in types)
            {
                // Kiểm tra tên Class kết thúc bằng "Repository" hoặc "Service"
                if (type.Name.EndsWith("Repository") || type.Name.EndsWith("Service"))
                {
                    // Ngoại lệ: Bỏ qua các class đặc biệt đăng ký bằng cách khác hoặc có vòng đời khác
                    if (type.Name == "ClassifyAIRepository" || type.Name == "MoMoService" || type.Name == "PasswordService" || type.Name == "CloudinaryService")
                    {
                        continue;
                    }

                    // Quy ước: Tên Interface bắt đầu bằng chữ 'I' + tên Class
                    var interfaceName = "I" + type.Name;
                    var matchingInterface = type.GetInterfaces()
                        .FirstOrDefault(i => i.Name == interfaceName);

                    // Đăng ký cặp Interface - Implementation dạng Scoped
                    if (matchingInterface != null)
                    {
                        services.AddScoped(matchingInterface, type);
                    }
                }
            }

            // ĐĂNG KÝ CÁC DỊCH VỤ NGOẠI LỆ (Đặc thù hoặc cần vòng đời khác)
            
            // PasswordService đăng ký là Singleton vì không giữ trạng thái động (state)
            services.AddSingleton<PasswordService>();
            
            // CloudinaryService cần đăng ký Scoped và ánh xạ cấu hình Settings
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
            
            // Cấu hình Google Authentication Settings
            services.Configure<GoogleAuthSettings>(configuration.GetSection("Authentication:Google"));

            // Đăng ký HttpClients đặc biệt
            services.AddHttpClient<IMoMoService, MoMoService>();
            services.AddHttpClient<IClassifyAIRepository, ClassifyAIRepository>(client =>
            {
                client.BaseAddress = new Uri(configuration["AI:BaseUrl"]);
            });

            return services;
        }
    }
}
