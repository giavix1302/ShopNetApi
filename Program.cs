using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopNetApi.Data;
using ShopNetApi.DTOs.Common;
using ShopNetApi.Models;
using ShopNetApi.Services;
using ShopNetApi.Services.Interfaces;
using ShopNetApi.Settings;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===================== Controllers =====================
builder.Services.AddControllers();

// ===================== Swagger =====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "ShopNetApi",
        Version = "v1"
    });
});

// ===================== Config response =====================
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value!.Errors.Count > 0)
            .Select(x => new
            {
                field = x.Key,
                message = x.Value!.Errors.First().ErrorMessage
            });

        return new BadRequestObjectResult(
            ApiResponse<object>.Fail(
                "Dữ liệu không hợp lệ",
                errors
            )
        );
    };
});

// ===================== EF Core =====================
// (thêm khi làm migration)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
// ===================== Identity =====================
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<long>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
});

// ===================== JWT =====================
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();

var jwtConfig = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
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

// ===================== AutoMapper =====================
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ===================== REDIS + SMTP =====================
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(
        builder.Configuration["Redis:ConnectionString"]!
    )
);

builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("Smtp")
);

builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<OtpService>();

builder.Services.AddScoped<RefreshTokenService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedRoles(scope.ServiceProvider);
}

// ===================== Middleware =====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShopNetApi v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
