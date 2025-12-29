using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShopNetApi.DTOs.Auth;
using ShopNetApi.DTOs.Common;
using ShopNetApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShopNetApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {

            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !user.Enabled)
                return Unauthorized(
                    ApiResponse<object>.Fail("Invalid credentials")
                );

            var validPassword = await _userManager
                .CheckPasswordAsync(user, dto.Password);

            if (!validPassword)
                return Unauthorized(
                    ApiResponse<object>.Fail("Invalid credentials")
                );

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256)
            );

            return Ok(
                ApiResponse<object>.Ok("Đăng nhập thành công", new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                })
             );
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {

            var userByEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (userByEmail != null)
                return BadRequest(
                    ApiResponse<object>.Fail("Email đã tồn tại")
                );

            var user = new ApplicationUser
            {
                Email = dto.Email,
                FullName = dto.FullName,
                Enabled = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "Đăng ký thất bại",
                        result.Errors.Select(e => new
                        {
                            code = e.Code,
                            description = e.Description
                        })
                    )
                );
            }

            await _userManager.AddToRoleAsync(user, "User");

            return Ok(
                ApiResponse<object>.Ok("Đăng ký thành công")
            );
        }
    }
}
