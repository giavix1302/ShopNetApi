using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopNetApi.DTOs.Auth;
using ShopNetApi.DTOs.Common;
using ShopNetApi.Models;
using ShopNetApi.Services;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly OtpService _otpService;
        private readonly EmailService _emailService;
        private readonly IAuthService _authService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            OtpService otpService,
            EmailService emailService,
            IAuthService authService
            )
        {
            _userManager = userManager;
            _config = config;
            _otpService = otpService;
            _emailService = emailService;
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !user.Enabled)
                return Unauthorized(
                    ApiResponse<object>.Fail("Invalid credentials")
                );

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized(
                    ApiResponse<object>.Fail("Invalid credentials")
                );


            string accessToken = await _authService.SignInAsync(user);

            return Ok(
                ApiResponse<object>.Ok("Đăng nhập thành công", new
                {
                    accessToken
                })
            );
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                return BadRequest(
                    ApiResponse<object>.Fail("Email đã tồn tại")
                );

            // Generate OTP + lưu email + fullName vào Redis
            var otp = await _otpService.GenerateAndStoreAsync(
                dto.Email,
                dto.FullName!
            );

            // Gửi OTP qua email
            await _emailService.SendOtpAsync(dto.Email, otp);

            return Ok(
                ApiResponse<object>.Ok(
                    "OTP đã được gửi tới email. Vui lòng xác thực để hoàn tất đăng ký"
                )
            );
        }

        [AllowAnonymous]
        [HttpPost("verify-register-otp")]
        public async Task<IActionResult> VerifyRegisterOtp(VerifyOtpDto dto)
        {
            var otpResult = await _otpService.VerifyAsync(dto.Email, dto.Otp);

            if (otpResult == null)
                return BadRequest(
                    ApiResponse<object>.Fail("OTP không hợp lệ hoặc đã hết hạn")
                );

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
                return BadRequest(
                    ApiResponse<object>.Fail("Đăng ký thất bại", result.Errors)
                );

            await _userManager.AddToRoleAsync(user, "User");

            string accessToken = await _authService.SignInAsync(user);

            return Ok(
                ApiResponse<object>.Ok("Đăng ký & đăng nhập thành công", new
                {
                    accessToken
                })
            );
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(ApiResponse<object>.Fail("Missing refresh token"));

            var accessToken = await _authService.RefreshAsync(refreshToken);

            if (accessToken == null)
                return Unauthorized(ApiResponse<object>.Fail("Invalid refresh token"));

            return Ok(ApiResponse<object>.Ok("Refresh thành công", new
            {
                accessToken
            }));
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
                await _authService.LogoutAsync(refreshToken);

            // Xóa cookie
            Response.Cookies.Delete("refreshToken");

            return Ok(
                ApiResponse<object>.Ok("Logout thành công")
            );
        }


    }
}
