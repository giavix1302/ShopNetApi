using ShopNetApi.DTOs.ProductImage;

namespace ShopNetApi.Services.Interfaces
{
    public interface IProductImageService
    {
        Task<ProductImageResponseDto> CreateAsync(CreateProductImageDto dto);
        Task<ProductImageResponseDto> UpdateAsync(long id, UpdateProductImageDto dto);
        Task DeleteAsync(long id);
        Task<List<ProductImageResponseDto>> GetByProductAsync(long productId);
    }
}
