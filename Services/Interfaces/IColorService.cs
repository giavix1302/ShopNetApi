using ShopNetApi.DTOs.Color;

namespace ShopNetApi.Services.Interfaces
{
    public interface IColorService
    {
        Task<ColorResponseDto> CreateAsync(CreateColorDto dto);
        Task<ColorResponseDto> UpdateAsync(long id, UpdateColorDto dto);
        Task DeleteAsync(long id);
        Task<List<ColorResponseDto>> GetAllAsync();
        Task<ColorResponseDto> GetByIdAsync(long id);
    }
}
