using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShopNetApi.DTOs.Color;
using ShopNetApi.DTOs.Common;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Controllers
{
    [ApiController]
    [Route("api/colors")]
    public class ColorsController : ControllerBase
    {
        private readonly IColorService _colorService;

        public ColorsController(IColorService colorService)
        {
            _colorService = colorService;
        }

        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("AdminCategoryPolicy")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateColorDto dto)
        {
            var result = await _colorService.CreateAsync(dto);

            return Ok(ApiResponse<ColorResponseDto>.Ok(
                "Color created successfully",
                result
            ));
        }

        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("AdminCategoryPolicy")]
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateColorDto dto)
        {
            var result = await _colorService.UpdateAsync(id, dto);

            return Ok(ApiResponse<ColorResponseDto>.Ok(
                "Color updated successfully",
                result
            ));
        }

        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("AdminCategoryPolicy")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _colorService.DeleteAsync(id);

            return Ok(ApiResponse<object>.Ok(
                "Color deleted successfully"
            ));
        }

        [AllowAnonymous]
        [EnableRateLimiting("PublicReadPolicy")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _colorService.GetAllAsync();

            return Ok(ApiResponse<List<ColorResponseDto>>.Ok(
                "Get colors successfully",
                result
            ));
        }

        [AllowAnonymous]
        [EnableRateLimiting("PublicReadPolicy")]
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _colorService.GetByIdAsync(id);

            return Ok(ApiResponse<ColorResponseDto>.Ok(
                "Get color successfully",
                result
            ));
        }
    }
}
