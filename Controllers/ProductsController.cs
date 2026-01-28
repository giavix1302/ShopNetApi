using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNetApi.DTOs.Common;
using ShopNetApi.DTOs.Product;
using ShopNetApi.DTOs.Review;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IReviewService _reviewService;

        public ProductsController(IProductService productService, IReviewService reviewService)
        {
            _productService = productService;
            _reviewService = reviewService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var result = await _productService.CreateAsync(dto);
            return Ok(ApiResponse<ProductResponseDto>.Ok(
                "Product created successfully", result));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateProductDto dto)
        {
            var result = await _productService.UpdateAsync(id, dto);
            return Ok(ApiResponse<ProductResponseDto>.Ok(
                "Product updated successfully", result));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _productService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(
                "Product deleted successfully", null));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _productService.GetAllAsync();
            return Ok(ApiResponse<List<ProductResponseDto>>.Ok(
                "Get products successfully", result));
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _productService.GetByIdAsync(id);
            return Ok(ApiResponse<ProductResponseDto>.Ok(
                "Get product successfully", result));
        }

        [HttpGet("{productId:long}/reviews")]
        public async Task<IActionResult> GetProductReviews(long productId, [FromQuery] ReviewQueryDto query)
        {
            var result = await _reviewService.GetProductReviewsAsync(productId, query);
            return Ok(ApiResponse<ReviewListPaginatedResponseDto>.Ok(
                "Lấy danh sách review thành công", result));
        }

        [HttpGet("{productId:long}/reviews/stats")]
        public async Task<IActionResult> GetProductReviewStats(long productId)
        {
            var result = await _reviewService.GetProductReviewStatsAsync(productId);
            return Ok(ApiResponse<ReviewStatsResponseDto>.Ok(
                "Lấy thống kê review thành công", result));
        }
    }
}
