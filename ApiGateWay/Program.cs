
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// ======================
// CONFIG
// ======================

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();


// ======================
// JWT
// ======================


var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"];

if (string.IsNullOrEmpty(secretKey))
{
    // Thông báo lỗi rõ ràng thay vì để nó crash khó hiểu
    throw new InvalidOperationException("JWT Key is missing in configuration. Check appsettings.json.");
}

var key = Encoding.UTF8.GetBytes(secretKey);

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

builder.Services.AddAuthorization();
// ======================
// CORS
// ======================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// ======================
// OCELOT + POLLY
// ======================

builder.Services
    .AddOcelot()
    .AddPolly();

var app = builder.Build();


// ======================
// CORS
// ======================

app.UseCors("AllowAll");


// ======================
// AUTHENTICATION
// ======================

app.UseAuthentication();
app.UseAuthorization();


// ======================
// TRACE ID
// ======================

app.Use(async (context, next) =>
{
    var traceId = Guid.NewGuid().ToString();

    context.Request.Headers["X-Trace-Id"] = traceId;

    Console.WriteLine(
        $"[{traceId}] " +
        $"{context.Request.Method} " +
        $"{context.Request.Path}");

    await next();
});



// GLOBAL ROUTE


app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();

    var publicRoutes = new[]
    {
        "/api/auth/",
        "/api/staff/auth/staff-login",
        "/product-service",
        "/api/",
        "/api/auth/register",
        
    };

    var isPublic =
        publicRoutes.Any(x =>
            path != null &&
            path.StartsWith(x));

    if (!isPublic)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Response.StatusCode = 401;

            await context.Response.WriteAsync("Unauthorized");

            return;
        }
    }

    await next();
});



// ROLE CHECK FOR DOWNSTREAM URL


var rolePermissions =
    new Dictionary<string, string[]>
{

    //{ "/inventory", new[] { "Admin", "Staff" } },
    //{ "/voucher-service/{everything}", new[] { "Admin" } },
    

};

app.Use(async (context, next) =>
{
    var path =
        context.Request.Path.Value?.ToLower();

    var role =
        context.User.Claims
            .FirstOrDefault(x =>
                x.Type == ClaimTypes.Role ||
                x.Type == "role")
            ?.Value;

    foreach (var permission in rolePermissions)
    {
        if (path!.StartsWith(permission.Key))
        {
            if (!permission.Value.Contains(role))
            {
                context.Response.StatusCode = 403;

                await context.Response.WriteAsync("Forbidden");

                return;
            }
        }
    }

    await next();
});



// PARSE CLAIMS -> HEADERS


app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var userId =
            context.User.Claims
                .FirstOrDefault(x =>
                    x.Type == ClaimTypes.NameIdentifier ||
                    x.Type == "sub")
                ?.Value;

        var role =
            context.User.Claims
                .FirstOrDefault(x =>
                    x.Type == ClaimTypes.Role ||
                    x.Type == "role")
                ?.Value;

        // LOG CLAIMS
        Console.WriteLine("========== JWT CLAIMS ==========");
        Console.WriteLine($"UserId: {userId}");
        Console.WriteLine($"Role: {role}");

        foreach (var claim in context.User.Claims)
        {
            Console.WriteLine(
                $"Claim Type: {claim.Type} | Value: {claim.Value}");
        }

        Console.WriteLine("================================");

        // ADD HEADERS
        context.Request.Headers["X-User-Id"] =
            userId ?? "";

        context.Request.Headers["X-Role"] =
            role ?? "";
    }
    else
    {
        Console.WriteLine("User NOT authenticated");
    }

    await next();
});


// ======================
// OCELOT
// ======================

await app.UseOcelot();

app.Run();