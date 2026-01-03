using ShopNetApi.Models;

namespace ShopNetApi.Repositories.Interfaces
{
    public interface IProductImageRepository
    {
        Task AddAsync(ProductImage image);
        Task UpdateAsync(ProductImage image);
        Task DeleteAsync(ProductImage image);
        Task<ProductImage?> GetByIdAsync(long id);
        Task<List<ProductImage>> GetByProductIdAsync(long productId);
        Task<ProductImage?> GetPrimaryAsync(long productId);
        Task<bool> AnyByProductAsync(long productId);
        Task UnsetPrimaryAsync(long productId);
    }
}
