using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShopNetApi.DTOs.Common;
using ShopNetApi.DTOs.User.Admin;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUsersController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        [EnableRateLimiting("AdminReadPolicy")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] AdminUserQueryDto query)
        {
            var result = await _adminUserService.GetUsersAsync(query);
            return Ok(ApiResponse<AdminUserListResponseDto>.Ok(
                "Lấy danh sách người dùng thành công", result));
        }

        [EnableRateLimiting("AdminReadPolicy")]
        [HttpGet("{userId:long}")]
        public async Task<IActionResult> GetById(long userId)
        {
            var result = await _adminUserService.GetUserByIdAsync(userId);
            return Ok(ApiResponse<AdminUserDetailDto>.Ok(
                "Lấy thông tin người dùng thành công", result));
        }
    }
}
