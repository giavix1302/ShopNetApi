using ShopNetApi.Models;

namespace ShopNetApi.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<bool> ExistsBySlugAsync(string slug, long? excludeId = null);

        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Product product);

        Task<Product?> GetByIdAsync(long id);
        Task<Product?> GetByIdWithRelationsAsync(long id);
        Task<List<Product>> GetAllAsync();

        Task<bool> HasSpecificationsAsync(long productId);
        Task ReplaceSpecificationsAsync(
            Product product,
            List<ProductSpecification> specs
        );
    }
}
