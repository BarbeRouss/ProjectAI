using System.Text;
using System.Threading.RateLimiting;
using HouseFlow.API.Middleware;
using HouseFlow.Application.Interfaces;
using HouseFlow.Infrastructure.Data;
using HouseFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Database
if (builder.Environment.EnvironmentName == "Testing")
{
    builder.Services.AddDbContext<HouseFlowDbContext>(options =>
        options.UseInMemoryDatabase("InMemoryTestDb"));
}
else
{
    // Aspire adds the connection string automatically with the name "houseflow"
    builder.AddNpgsqlDbContext<HouseFlowDbContext>("houseflow");
}

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHouseService, HouseService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();

// JWT Authentication
// JWT Key priority: 1. Environment variable 2. Configuration file 3. User secrets
var jwtKey = Environment.GetEnvironmentVariable("JWT__KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key not configured. Set JWT__KEY environment variable or Jwt:Key in configuration.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer not configured");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience not configured");

// Validate JWT key strength (minimum 256 bits = 32 bytes)
if (jwtKey.Length < 32)
{
    throw new InvalidOperationException("JWT Key must be at least 32 characters (256 bits) for security.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // Remove default 5-minute tolerance
        };
    });

builder.Services.AddAuthorization();

// NSwag OpenAPI
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "HouseFlow API";
    config.Version = "v1";
    config.Description = "API for the HouseFlow Home Maintenance Application";
    config.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Rate Limiting (disabled for Development and Testing environments to allow E2E tests)
if (!builder.Environment.IsDevelopment() && builder.Environment.EnvironmentName != "Testing")
{
    builder.Services.AddRateLimiter(options =>
    {
        // Auth endpoints: 5 requests per minute (prevent brute force)
        options.AddFixedWindowLimiter("auth", limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0; // No queueing
        });

        // API endpoints: 100 requests per minute
        options.AddFixedWindowLimiter("api", limiterOptions =>
        {
            limiterOptions.PermitLimit = 100;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 10;
        });

        // Global fallback: 200 requests per minute
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 200,
                    QueueLimit = 0,
                    Window = TimeSpan.FromMinutes(1)
                }));

        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = 429;

            double? retryAfterSeconds = null;
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                retryAfterSeconds = retryAfter.TotalSeconds;
            }

            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded. Please try again later.",
                retryAfter = retryAfterSeconds
            }, cancellationToken: token);
        };
    });
}

var app = builder.Build();

// Apply pending migrations automatically in Development mode
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<HouseFlowDbContext>();
        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();

// Security headers middleware
app.UseMiddleware<SecurityHeadersMiddleware>();

// Rate limiter middleware (only if rate limiting is configured)
if (!app.Environment.IsDevelopment() && app.Environment.EnvironmentName != "Testing")
{
    app.UseRateLimiter();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
