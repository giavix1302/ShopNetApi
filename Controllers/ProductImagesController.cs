using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShopNetApi.DTOs.Common;
using ShopNetApi.DTOs.ProductImage;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Controllers
{
    [ApiController]
    [Route("api/products/{productId:long}/images")]
    public class ProductImagesController : ControllerBase
    {
        private readonly IProductImageService _service;

        public ProductImagesController(IProductImageService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("AdminImagePolicy")]
        [HttpPost]
        public async Task<IActionResult> Create(
            long productId,
            [FromForm] CreateProductImageDto dto)
        {
            dto.ProductId = productId;

            var result = await _service.CreateAsync(dto);

            return Ok(ApiResponse<ProductImageResponseDto>.Ok(
                "Product image created successfully", result));
        }

        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("AdminImagePolicy")]
        [HttpPut("{imageId:long}")]
        public async Task<IActionResult> Update(
            long productId,
            long imageId,
            [FromForm] UpdateProductImageDto dto)
        {
            var result = await _service.UpdateAsync(productId, imageId, dto);

            return Ok(ApiResponse<ProductImageResponseDto>.Ok(
                "Product image updated successfully", result));
        }

        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("AdminImagePolicy")]
        [HttpDelete("{imageId:long}")]
        public async Task<IActionResult> Delete(
            long productId,
            long imageId)
        {
            await _service.DeleteAsync(productId, imageId);

            return Ok(ApiResponse<object>.Ok(
                "Product image deleted successfully", null));
        }

        [EnableRateLimiting("PublicReadPolicy")]
        [HttpGet]
        public async Task<IActionResult> GetAll(long productId)
        {
            var result = await _service.GetByProductAsync(productId);

            return Ok(ApiResponse<List<ProductImageResponseDto>>.Ok(
                "Get product images successfully", result));
        }
    }
}
