using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using ShopNetApi.Data;
using ShopNetApi.DTOs.Common;
using ShopNetApi.Middlewares;
using ShopNetApi.Models;
using ShopNetApi.Repositories;
using ShopNetApi.Repositories.Interfaces;
using ShopNetApi.Services;
using ShopNetApi.Services.Interfaces;
using ShopNetApi.Settings;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 1. LOGGING (SERILOG)
// ==========================================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ==========================================================
// 2. DATABASE & IDENTITY (ENTITY FRAMEWORK)
// ==========================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<long>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
});

// ==========================================================
// 3. AUTHENTICATION & AUTHORIZATION (JWT)
// ==========================================================
builder.Services.AddHttpContextAccessor();
var jwtConfig = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.UseSecurityTokenValidators = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig["Issuer"],
        ValidAudience = jwtConfig["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

// ==========================================================
// RATE LIMITING
// ==========================================================
builder.Services.AddRateLimiter(options =>
{
    // Global limiter: 200 requests/phút/IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 200,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Format rate limit response - viết trực tiếp để IDE không xóa using
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        var response = ApiResponse<object>.Fail("Quá nhiều requests. Vui lòng thử lại sau.", null);
        await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
    };

    // Helper function để lấy partition key theo IP
    Func<HttpContext, string> getIpPartitionKey = (context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    // Helper function để lấy partition key theo UserId
    Func<HttpContext, string> getUserPartitionKey = (context) =>
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    };

    // Helper function để lấy partition key cho Admin
    Func<HttpContext, string> getAdminPartitionKey = (context) =>
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = context.User.IsInRole("Admin");
        return isAdmin ? $"admin_{userId}" : (userId ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown");
    };

    // ============ AUTHENTICATION APIs (Sliding Window) ============

    // Login - 5 requests/phút/IP (Sliding Window)
    options.AddPolicy("LoginPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: getIpPartitionKey(context),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Register - 3 requests/phút/IP (Sliding Window)
    options.AddPolicy("RegisterPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: getIpPartitionKey(context),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Verify OTP - 10 requests/phút/IP (Sliding Window)
    options.AddPolicy("VerifyOtpPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: getIpPartitionKey(context),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Refresh Token - 20 requests/phút/IP (Sliding Window)
    options.AddPolicy("RefreshPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: getIpPartitionKey(context),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Refresh Admin - 30 requests/phút/User (Sliding Window)
    options.AddPolicy("RefreshAdminPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: getUserPartitionKey(context),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Logout - 10 requests/phút/User (Sliding Window)
    options.AddPolicy("LogoutPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: getUserPartitionKey(context),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // ============ WRITE OPERATIONS (Sliding Window) ============

    // Create Order - 10 requests/phút/User (Sliding Window)
    options.AddPolicy("CreateOrderPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: getUserPartitionKey(context),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Cancel Order - 5 requests/phút/User (Sliding Window)
    options.AddPolicy("CancelOrderPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: getUserPartitionKey(context),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Create Review - 5 requests/phút/User (Sliding Window)
    options.AddPolicy("CreateReviewPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: getUserPartitionKey(context),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Cart Operations - 30 requests/phút/User (Sliding Window)
    options.AddPolicy("CartWritePolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: getUserPartitionKey(context),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // ============ ADMIN WRITE OPERATIONS (Sliding Window) ============

    // Admin Product Operations - 20 requests/phút/Admin (Sliding Window)
    options.AddPolicy("AdminProductPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: getAdminPartitionKey(context),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Admin Order Operations - 30 requests/phút/Admin (Sliding Window)
    options.AddPolicy("AdminOrderPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: getAdminPartitionKey(context),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Admin Category/Brand/Color Operations - 15 requests/phút/Admin (Sliding Window)
    options.AddPolicy("AdminCategoryPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: getAdminPartitionKey(context),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 15,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Admin Image Operations - 20 requests/phút/Admin (Sliding Window)
    options.AddPolicy("AdminImagePolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: getAdminPartitionKey(context),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // ============ READ OPERATIONS (Fixed Window) ============

    // Public Read - 100 requests/phút/IP (Fixed Window)
    options.AddPolicy("PublicReadPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: getIpPartitionKey(context),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Authenticated Read - 60 requests/phút/User (Fixed Window)
    options.AddPolicy("AuthenticatedReadPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: getUserPartitionKey(context),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Admin Read - 100 requests/phút/Admin (Fixed Window)
    options.AddPolicy("AdminReadPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: getAdminPartitionKey(context),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Concurrency Limit - 10 requests đồng thời/IP
    options.AddPolicy("ConcurrencyPolicy", context =>
        RateLimitPartition.GetConcurrencyLimiter(
            partitionKey: getIpPartitionKey(context),
            factory: partition => new ConcurrencyLimiterOptions
            {
                PermitLimit = 10,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

});

// ==========================================================
// 4. INFRASTRUCTURE (REDIS, SMTP, AUTOMAPPER, SWAGGER)
// ==========================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Allow enums to be sent/received as strings, e.g. \"COD\" instead of 0
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "ShopNetApi", Version = "v1" }));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]!));

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));

// Custom Validation Response
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value!.Errors.Count > 0)
            .Select(x => new { field = x.Key, message = x.Value!.Errors.First().ErrorMessage });
        return new BadRequestObjectResult(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", errors));
    };
});

// ==========================================================
// 5. DEPENDENCY INJECTION (REPOSITORIES & SERVICES)
// ==========================================================
// Repositories
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
builder.Services.AddScoped<IColorRepository, ColorRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

// Services
builder.Services.AddScoped<IEmailService, EmailService>(); // Khuyên dùng Interface
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductImageService, ProductImageService>();
builder.Services.AddScoped<IColorService, ColorService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IAdminReviewService, AdminReviewService>();


// ==========================================================
// 6. PIPELINE & MIDDLEWARES
// ==========================================================
var app = builder.Build();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedRoles(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShopNetApi v1"));
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();