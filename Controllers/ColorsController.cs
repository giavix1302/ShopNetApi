using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // ================= CREATE =================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateColorDto dto)
        {
            var result = await _colorService.CreateAsync(dto);

            return Ok(ApiResponse<ColorResponseDto>.Ok(
                "Color created successfully",
                result
            ));
        }

        // ================= UPDATE =================
        [HttpPut("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateColorDto dto)
        {
            var result = await _colorService.UpdateAsync(id, dto);

            return Ok(ApiResponse<ColorResponseDto>.Ok(
                "Color updated successfully",
                result
            ));
        }

        // ================= DELETE =================
        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(long id)
        {
            await _colorService.DeleteAsync(id);

            return Ok(ApiResponse<object>.Ok(
                "Color deleted successfully"
            ));
        }

        // ================= GET ALL =================
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var result = await _colorService.GetAllAsync();

            return Ok(ApiResponse<List<ColorResponseDto>>.Ok(
                "Get colors successfully",
                result
            ));
        }

        // ================= GET BY ID =================
        [HttpGet("{id:long}")]
        [AllowAnonymous]
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
