using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShopNetApi.DTOs.Common;
using ShopNetApi.DTOs.Review;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [EnableRateLimiting("CreateReviewPolicy")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
        {
            var result = await _reviewService.CreateAsync(dto);
            return Ok(ApiResponse<ReviewResponseDto>.Ok(
                "Tạo review thành công", result));
        }

        [EnableRateLimiting("AuthenticatedReadPolicy")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyReviews([FromQuery] ReviewQueryDto? query)
        {
            var result = await _reviewService.GetMyReviewsAsync(query);
            return Ok(ApiResponse<ReviewListPaginatedResponseDto>.Ok(
                "Lấy danh sách review thành công", result));
        }

        [EnableRateLimiting("AuthenticatedReadPolicy")]
        [HttpGet("me/{reviewId:long}")]
        public async Task<IActionResult> GetMyReviewById(long reviewId)
        {
            var result = await _reviewService.GetMyReviewByIdAsync(reviewId);
            return Ok(ApiResponse<ReviewResponseDto>.Ok(
                "Lấy thông tin review thành công", result));
        }

        [EnableRateLimiting("CartWritePolicy")]
        [HttpPut("{reviewId:long}")]
        public async Task<IActionResult> Update(long reviewId, [FromBody] UpdateReviewDto dto)
        {
            var result = await _reviewService.UpdateAsync(reviewId, dto);
            return Ok(ApiResponse<ReviewResponseDto>.Ok(
                "Cập nhật review thành công", result));
        }

        [EnableRateLimiting("CartWritePolicy")]
        [HttpDelete("{reviewId:long}")]
        public async Task<IActionResult> Delete(long reviewId)
        {
            await _reviewService.DeleteAsync(reviewId);
            return Ok(ApiResponse<object>.Ok(
                "Xóa review thành công", null));
        }
    }
}
