using ShopNetApi.DTOs.Brand;

namespace ShopNetApi.Services.Interfaces
{
    public interface IBrandService
    {
        Task<BrandResponseDto> CreateAsync(CreateBrandDto dto);
        Task<BrandResponseDto> UpdateAsync(long id, UpdateBrandDto dto);
        Task DeleteAsync(long id);
        Task<List<BrandResponseDto>> GetAllAsync();
        Task<BrandResponseDto> GetByIdAsync(long id);
    }
}
