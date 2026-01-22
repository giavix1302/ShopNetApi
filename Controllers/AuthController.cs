using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNetApi.DTOs.Auth;
using ShopNetApi.DTOs.Common;
using ShopNetApi.Exceptions;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            return Ok(ApiResponse<LoginResponseDto>.Ok("Đăng nhập thành công", result));
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            await _authService.RegisterAsync(dto);

            return Ok(ApiResponse<object>.Ok(
                "OTP đã được gửi tới email. Vui lòng xác thực"
            ));
        }

        [AllowAnonymous]
        [HttpPost("verify-register-otp")]
        public async Task<IActionResult> VerifyRegisterOtp(VerifyOtpDto dto)
        {
            var token = await _authService.VerifyRegisterOtpAsync(dto);

            return Ok(ApiResponse<object>.Ok("Đăng ký & đăng nhập thành công", new
            {
                accessToken = token
            }));
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedException("Missing refresh token");

            var accessToken = await _authService.RefreshAsync(refreshToken);

            if (accessToken == null)
                throw new UnauthorizedException("Invalid refresh token");

            return Ok(
                ApiResponse<object>.Ok("Refresh thành công", new
                {
                    accessToken
                })
            );
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
                await _authService.LogoutAsync(refreshToken);

            Response.Cookies.Delete("refreshToken");
            return Ok(ApiResponse<object>.Ok("Logout thành công"));
        }


    }
}
