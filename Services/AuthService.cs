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
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            IHttpContextAccessor http,
            IRefreshTokenService refreshTokenService,
            IRefreshTokenRepository refreshTokenRepo,
            IOtpService otpService,
            IEmailService emailService,
            ICartRepository cartRepository,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _config = config;
            _http = http;
            _refreshTokenService = refreshTokenService;
            _refreshTokenRepo = refreshTokenRepo;
            _otpService = otpService;
            _emailService = emailService;
            _cartRepository = cartRepository;
            _logger = logger;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            _logger.LogInformation(
                "Login attempt. Email={Email}", dto.Email);

            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !user.Enabled)
            {
                _logger.LogWarning(
                    "Login failed. Email={Email}. Reason=User not found or disabled",
                    dto.Email);

                throw new UnauthorizedException("Invalid credentials");
            }


            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                _logger.LogWarning(
                    "Login failed. Email={Email}. Reason=Invalid password",
                    dto.Email);

                throw new UnauthorizedException("Invalid credentials");
            }

            _logger.LogInformation(
                "Login success. UserId={UserId}", user.Id);

            // Đảm bảo user có cart (cho trường hợp user cũ chưa có cart)
            var existingCart = await _cartRepository.GetByUserIdAsync(user.Id);
            if (existingCart == null)
            {
                await _cartRepository.AddAsync(new Cart
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                });
                _logger.LogInformation(
                    "Cart created for existing user. UserId={UserId}", user.Id);
            }

            var (accessToken, refreshToken) = await SignInAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var isAdmin = roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = isAdmin ? refreshToken : null,
                User = new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName,
                    Address = user.Address,
                    AvatarUrl = user.AvatarUrl,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt
                }
            };
        }

        // ================= REGISTER =================
        public async Task RegisterAsync(RegisterDto dto)
        {
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
            {
                _logger.LogWarning(
                    "Register failed. Email={Email}. Reason=Email already exists",
                    dto.Email);

                throw new BadRequestException("Email đã tồn tại");
            }
    
            var otp = await _otpService.GenerateAndStoreAsync(dto.Email, dto.FullName!);

            _logger.LogInformation(
                "Register OTP sent. Email={Email}", dto.Email);

            await _emailService.SendOtpAsync(dto.Email, otp);
        }

        // ================= VERIFY OTP =================
        public async Task<string> VerifyRegisterOtpAsync(VerifyOtpDto dto)
        {
            var otpResult = await _otpService.VerifyAsync(dto.Email, dto.Otp);
            if (otpResult == null)
            {
                _logger.LogWarning(
                    "Verify OTP failed. Email={Email}", dto.Email);

                throw new BadRequestException("OTP không hợp lệ hoặc đã hết hạn");
            }

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
            {
                _logger.LogError(
                    "User creation failed after OTP verification. Email={Email}",
                    dto.Email);

                throw new BadRequestException("Đăng ký thất bại");
            }

            await _userManager.AddToRoleAsync(user, "User");

            // Tạo cart trống cho user mới
            await _cartRepository.AddAsync(new Cart
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation(
                "User registered successfully. UserId={UserId}, Cart created", user.Id);

            var (accessToken, _) = await SignInAsync(user);
            return accessToken;
        }

        // ================= SIGN IN =================
        private async Task<(string accessToken, string refreshToken)> SignInAsync(ApplicationUser user, bool skipCookie = false)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!)
            };

            foreach (var r in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, r));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            _logger.LogInformation("Generating JWT token. UserId={UserId}", user.Id);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(120),
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

            if (!skipCookie)
            {
                SetCookie(refreshToken);
            }

            _logger.LogInformation("Refresh token generated and stored. UserId={UserId}", user.Id);

            return (accessToken, refreshToken);
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
            var userId = await _refreshTokenService.ValidateAsync(refreshToken);
            if (userId == null)
            {
                _logger.LogWarning("Refresh token validation failed");
                return null;
            }

            var tokenEntity = await _refreshTokenRepo.GetLatestValidAsync(userId.Value);

            if (tokenEntity == null)
            {
                _logger.LogWarning("Refresh token not found or revoked. UserId={UserId}", userId.Value);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(refreshToken, tokenEntity.TokenHash))
            {
                _logger.LogWarning(
                    "Refresh token hash mismatch. UserId={UserId}",
                    userId.Value);

                return null;
            }

            await _refreshTokenRepo.RevokeAsync(tokenEntity);

            var user = userId.HasValue
                ? await _userManager.FindByIdAsync(userId.Value.ToString())
                : null;

            if (user == null) return null;

            _logger.LogInformation(
                "Refresh token success. UserId={UserId}", user.Id);

            var (accessToken, _) = await SignInAsync(user); // trả accessToken mới
            return accessToken;
        }

        public async Task<RefreshResponseDto?> RefreshAdminAsync(string refreshToken)
        {
            var userId = await _refreshTokenService.ValidateAsync(refreshToken);
            if (userId == null)
            {
                _logger.LogWarning("Admin refresh token validation failed");
                return null;
            }

            var tokenEntity = await _refreshTokenRepo.GetLatestValidAsync(userId.Value);

            if (tokenEntity == null)
            {
                _logger.LogWarning("Admin refresh token not found or revoked. UserId={UserId}", userId.Value);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(refreshToken, tokenEntity.TokenHash))
            {
                _logger.LogWarning(
                    "Admin refresh token hash mismatch. UserId={UserId}",
                    userId.Value);

                return null;
            }

            var user = userId.HasValue
                ? await _userManager.FindByIdAsync(userId.Value.ToString())
                : null;

            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            var isAdmin = roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);

            if (!isAdmin)
            {
                _logger.LogWarning(
                    "Refresh admin called by non-admin user. UserId={UserId}",
                    userId.Value);
                return null;
            }

            await _refreshTokenRepo.RevokeAsync(tokenEntity);

            _logger.LogInformation(
                "Admin refresh token success. UserId={UserId}", user.Id);

            // Admin refresh không set cookie, chỉ trả về JSON
            var (accessToken, newRefreshToken) = await SignInAsync(user, skipCookie: true);

            return new RefreshResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var userId = await _refreshTokenService.ValidateAsync(refreshToken);
            if (userId == null)
            {
                _logger.LogWarning("Logout failed. Invalid refresh token");
                return;
            }

            var tokenEntity = await _refreshTokenRepo.GetLatestValidAsync(userId.Value);

            if (tokenEntity != null)
            {
                await _refreshTokenRepo.RevokeAsync(tokenEntity);
            }

            _logger.LogInformation("User logout. UserId={UserId}", userId.Value);

            await _refreshTokenService.RevokeAsync(refreshToken);
        }

    }
}
