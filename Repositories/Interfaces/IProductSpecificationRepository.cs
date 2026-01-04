using ShopNetApi.Models;

namespace ShopNetApi.Repositories.Interfaces
{
    public interface IProductSpecificationRepository
    {
        Task<bool> ExistsAsync(long productId, string specName, long? excludeId = null);
        Task AddAsync(ProductSpecification spec);
        Task<ProductSpecification?> GetByIdAsync(long id);
        Task<List<ProductSpecification>> GetByProductIdAsync(long productId);
        Task UpdateAsync(ProductSpecification spec);
        Task DeleteAsync(ProductSpecification spec);
    }
}
