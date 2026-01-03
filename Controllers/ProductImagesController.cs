using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // ========= CREATE =========
        [Authorize(Roles = "Admin")]
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

        // ========= UPDATE =========
        [Authorize(Roles = "Admin")]
        [HttpPut("{imageId:long}")]
        public async Task<IActionResult> Update(
            long productId,
            long imageId,
            [FromForm] UpdateProductImageDto dto)
        {
            var result = await _service.UpdateAsync(imageId, dto);

            return Ok(ApiResponse<ProductImageResponseDto>.Ok(
                "Product image updated successfully", result));
        }

        // ========= DELETE =========
        [Authorize(Roles = "Admin")]
        [HttpDelete("{imageId:long}")]
        public async Task<IActionResult> Delete(
            long productId,
            long imageId)
        {
            await _service.DeleteAsync(imageId);

            return Ok(ApiResponse<object>.Ok(
                "Product image deleted successfully", null));
        }

        // ========= GET ALL =========
        [HttpGet]
        public async Task<IActionResult> GetAll(long productId)
        {
            var result = await _service.GetByProductAsync(productId);

            return Ok(ApiResponse<List<ProductImageResponseDto>>.Ok(
                "Get product images successfully", result));
        }
    }
}
