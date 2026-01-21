using ShopNetApi.DTOs.ProductImage;

namespace ShopNetApi.Services.Interfaces
{
    public interface IProductImageService
    {
        Task<ProductImageResponseDto> CreateAsync(CreateProductImageDto dto);
        Task<ProductImageResponseDto> UpdateAsync(long productId, long imageId, UpdateProductImageDto dto);
        Task DeleteAsync(long productId, long imageId);
        Task<List<ProductImageResponseDto>> GetByProductAsync(long productId);
    }
}
