using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNetApi.DTOs.Brand;
using ShopNetApi.DTOs.Common;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Controllers
{
    [ApiController]
    [Route("api/brands")]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandService _brandService;
        public BrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandDto dto)
        {
            var result = await _brandService.CreateAsync(dto);

            return Ok(ApiResponse<BrandResponseDto>.Ok(
                "Brand created successfully",
                result
            ));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:long}")]
        public async Task<IActionResult> UpdateBrand(long id, [FromBody] UpdateBrandDto dto)
        {
            var result = await _brandService.UpdateAsync(id, dto);

            return Ok(ApiResponse<BrandResponseDto>.Ok(
                "Brand updated successfully",
                result
            ));
        }

        [Authorize(Roles = "Admin")]
        // ========= DELETE =========
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteBrand(long id)
        {
            await _brandService.DeleteAsync(id);

            return Ok(ApiResponse<object>.Ok(
                "Brand deleted successfully",
                null
            ));
        }

        // ========= GET ALL =========
        [HttpGet]
        public async Task<IActionResult> GetBrands()
        {
            var result = await _brandService.GetAllAsync();

            return Ok(ApiResponse<List<BrandResponseDto>>.Ok(
                "Get brands successfully",
                result
            ));
        }

        // ========= GET DETAIL =========
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetBrand(long id)
        {
            var result = await _brandService.GetByIdAsync(id);

            return Ok(ApiResponse<BrandResponseDto>.Ok(
                "Get brand successfully",
                result
            ));
        }
    }
}
