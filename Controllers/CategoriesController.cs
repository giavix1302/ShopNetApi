using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShopNetApi.DTOs.Category;
using ShopNetApi.DTOs.Common;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("AdminCategoryPolicy")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            var result = await _categoryService.CreateAsync(dto);

            return Ok(
                ApiResponse<CategoryResponseDto>.Ok("Category created successfully", result)
            );
        }

        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("AdminCategoryPolicy")]
        [HttpPut("{id:long}")]
        public async Task<IActionResult> UpdateCategory(
            long id,
            [FromBody] UpdateCategoryDto dto)
        {
            var result = await _categoryService.UpdateAsync(id, dto);

            return Ok(ApiResponse<CategoryResponseDto>.Ok(
                "Category updated successfully",
                result
            ));
        }

        [EnableRateLimiting("PublicReadPolicy")]
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _categoryService.GetAllAsync();

            return Ok(ApiResponse<List<CategoryResponseDto>>.Ok("Lấy danh sách loại thành công", result));
        }

        [EnableRateLimiting("PublicReadPolicy")]
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetCategory(long id)
        {
            var result = await _categoryService.GetByIdAsync(id);

            return Ok(ApiResponse<CategoryResponseDto>.Ok("Lấy chi tiết thành công", result));
        }

        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("AdminCategoryPolicy")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteCategory(long id)
        {
            await _categoryService.DeleteAsync(id);

            return Ok(ApiResponse<object>.Ok(
                "Category deleted successfully"
            ));
        }
    }
}
