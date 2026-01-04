using ShopNetApi.DTOs.ProductSpecification;

namespace ShopNetApi.Services.Interfaces
{
    public interface IProductSpecificationService
    {
        Task<ProductSpecificationResponseDto> CreateAsync(long productId, CreateProductSpecificationDto dto);

        Task<List<ProductSpecificationResponseDto>> GetByProductIdAsync(long productId);

        Task<ProductSpecificationResponseDto> UpdateAsync(long id, UpdateProductSpecificationDto dto);

        Task DeleteAsync(long id);
    }
}
