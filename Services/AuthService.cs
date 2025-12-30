using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopNetApi.Data;
using ShopNetApi.Models;
using ShopNetApi.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShopNetApi.Services
{
    public class AuthService : IAuthService
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly ApplicationDbContext _db;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            RefreshTokenService refreshTokenService,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _refreshTokenService = refreshTokenService;
            _db = db;
        }

        public async Task<string> SignInAsync(ApplicationUser user)
        {
            // ===== CLAIMS =====
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? user.UserName!)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            // ===== ACCESS TOKEN =====
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var accessToken = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256)
            );

            var accessTokenString =
                new JwtSecurityTokenHandler().WriteToken(accessToken);

            // ===== REFRESH TOKEN =====
            var refreshToken = _refreshTokenService.GenerateRefreshToken();
            var refreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);

            // ===== DB =====
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refreshTokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                IpAddress = _httpContextAccessor.HttpContext?
                    .Connection.RemoteIpAddress?.ToString()
            };

            _db.RefreshTokens.Add(refreshTokenEntity);
            await _db.SaveChangesAsync();

            // ===== REDIS =====
            await _refreshTokenService.SaveAsync(
                refreshToken,
                user.Id,
                TimeSpan.FromDays(7)
            );

            // ===== COOKIE =====
            SetTokenCookie("refreshToken", refreshToken, 7 * 24 * 60);
            return accessTokenString;
        }

        private void SetTokenCookie(string name, string value, int expireMinutes)
        {
            var response = _httpContextAccessor.HttpContext!.Response;

            response.Cookies.Append(name, value, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(expireMinutes)
            });
        }

        public async Task<string?> RefreshAsync(string refreshToken)
        {
            var hash = _refreshTokenService.HashToken(refreshToken);

            var userId = await _refreshTokenService.ValidateAsync(hash);
            if (userId == null)
                return null;

            var tokenEntity = await _db.RefreshTokens
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsRevoked &&
                    x.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (tokenEntity == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(refreshToken, tokenEntity.TokenHash))
                return null;

            tokenEntity.IsRevoked = true;
            await _db.SaveChangesAsync();

            var user = userId.HasValue
                ? await _userManager.FindByIdAsync(userId.Value.ToString())
                : null;

            if (user == null) return null;

            return await SignInAsync(user); // trả accessToken mới
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var userId = await _refreshTokenService.ValidateAsync(refreshToken);
            if (userId == null)
                return;

            var tokenEntity = await _db.RefreshTokens
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsRevoked &&
                    x.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (tokenEntity != null)
            {
                tokenEntity.IsRevoked = true;
                await _db.SaveChangesAsync();
            }

            await _refreshTokenService.RevokeAsync(refreshToken);
        }

    }
}
