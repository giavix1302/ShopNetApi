using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ShopNetApi.DTOs.Auth;
using ShopNetApi.Exceptions;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;
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
        private readonly IHttpContextAccessor _http;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly OtpService _otpService;
        private readonly EmailService _emailService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            IHttpContextAccessor http,
            RefreshTokenService refreshTokenService,
            IRefreshTokenRepository refreshTokenRepo,
            OtpService otpService,
            EmailService emailService)
        {
            _userManager = userManager;
            _config = config;
            _http = http;
            _refreshTokenService = refreshTokenService;
            _refreshTokenRepo = refreshTokenRepo;
            _otpService = otpService;
            _emailService = emailService;
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !user.Enabled)
                throw new UnauthorizedException("Invalid credentials");

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedException("Invalid credentials");

            return await SignInAsync(user);
        }

        // ================= REGISTER =================
        public async Task RegisterAsync(RegisterDto dto)
        {
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                throw new BadRequestException("Email đã tồn tại");

            var otp = await _otpService.GenerateAndStoreAsync(dto.Email, dto.FullName!);
            await _emailService.SendOtpAsync(dto.Email, otp);
        }

        // ================= VERIFY OTP =================
        public async Task<string> VerifyRegisterOtpAsync(VerifyOtpDto dto)
        {
            var otpResult = await _otpService.VerifyAsync(dto.Email, dto.Otp);
            if (otpResult == null)
                throw new BadRequestException("OTP không hợp lệ hoặc đã hết hạn");

            var user = new ApplicationUser
            {
                Email = otpResult.Email,
                UserName = otpResult.Email,
                FullName = otpResult.FullName,
                Enabled = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new BadRequestException("Đăng ký thất bại");

            await _userManager.AddToRoleAsync(user, "User");

            return await SignInAsync(user);
        }

        // ================= SIGN IN =================
        private async Task<string> SignInAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? user.UserName!)
            };

            foreach (var r in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, r));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // refresh token
            var refreshToken = _refreshTokenService.GenerateRefreshToken();
            var hash = BCrypt.Net.BCrypt.HashPassword(refreshToken);

            await _refreshTokenRepo.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = hash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IpAddress = _http.HttpContext?.Connection.RemoteIpAddress?.ToString()
            });

            await _refreshTokenService.SaveAsync(refreshToken, user.Id, TimeSpan.FromDays(7));
            SetCookie(refreshToken);

            return accessToken;
        }

        private void SetCookie(string token)
        {
            _http.HttpContext!.Response.Cookies.Append("refreshToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
        }

        public async Task<string?> RefreshAsync(string refreshToken)
        {
            var hash = _refreshTokenService.HashToken(refreshToken);

            var userId = await _refreshTokenService.ValidateAsync(hash);
            if (userId == null)
                return null;

            var tokenEntity = await _refreshTokenRepo.GetLatestValidAsync(userId.Value);

            if (tokenEntity == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(refreshToken, tokenEntity.TokenHash))
                return null;

            await _refreshTokenRepo.RevokeAsync(tokenEntity);

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

            var tokenEntity = await _refreshTokenRepo.GetLatestValidAsync(userId.Value);

            if (tokenEntity != null)
            {
                await _refreshTokenRepo.RevokeAsync(tokenEntity);
            }

            await _refreshTokenService.RevokeAsync(refreshToken);
        }

    }
}
