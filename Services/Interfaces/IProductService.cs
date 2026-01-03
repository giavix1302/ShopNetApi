using ShopNetApi.DTOs.Product;

namespace ShopNetApi.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponseDto> CreateAsync(CreateProductDto dto);
        Task<ProductResponseDto> UpdateAsync(long id, UpdateProductDto dto);
        Task DeleteAsync(long id);

        Task<List<ProductResponseDto>> GetAllAsync();
        Task<ProductResponseDto> GetByIdAsync(long id);
    }
}
