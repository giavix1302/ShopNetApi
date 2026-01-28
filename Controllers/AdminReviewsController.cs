using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNetApi.DTOs.Common;
using ShopNetApi.DTOs.Review.Admin;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Controllers
{
    [ApiController]
    [Route("api/admin/reviews")]
    [Authorize(Roles = "Admin")]
    public class AdminReviewsController : ControllerBase
    {
        private readonly IAdminReviewService _adminReviewService;

        public AdminReviewsController(IAdminReviewService adminReviewService)
        {
            _adminReviewService = adminReviewService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] AdminReviewQueryDto query)
        {
            var result = await _adminReviewService.GetReviewsAsync(query);
            return Ok(ApiResponse<AdminReviewListResponseDto>.Ok(
                "Lấy danh sách review thành công", result));
        }

        [HttpGet("{reviewId:long}")]
        public async Task<IActionResult> GetById(long reviewId)
        {
            var result = await _adminReviewService.GetReviewByIdAsync(reviewId);
            return Ok(ApiResponse<AdminReviewDetailDto>.Ok(
                "Lấy thông tin review thành công", result));
        }

        [HttpDelete("{reviewId:long}")]
        public async Task<IActionResult> Delete(long reviewId)
        {
            await _adminReviewService.DeleteReviewAsync(reviewId);
            return Ok(ApiResponse<object>.Ok(
                "Xóa review thành công", null));
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var result = await _adminReviewService.GetStatsAsync(from, to);
            return Ok(ApiResponse<AdminReviewStatsResponseDto>.Ok(
                "Lấy thống kê review thành công", result));
        }
    }
}
