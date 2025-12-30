using ShopNetApi.DTOs.Category;

namespace ShopNetApi.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto);

        Task<CategoryResponseDto> UpdateAsync(long id, UpdateCategoryDto dto);

        Task<List<CategoryResponseDto>> GetAllAsync();

        Task<CategoryResponseDto> GetByIdAsync(long id);

        Task DeleteAsync(long id);
    }
}
