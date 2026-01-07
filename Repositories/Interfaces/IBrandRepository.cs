using ShopNetApi.Models;

namespace ShopNetApi.Repositories.Interfaces
{
    public interface IBrandRepository
    {
        Task<bool> ExistsByNameAsync(string name, long? excludeId = null);
        Task AddAsync(Brand brand);
        Task<Brand?> GetByIdAsync(long id);
        Task<Brand?> GetByIdWithProductsAsync(long id);
        Task<List<Brand>> GetAllAsync();
        Task UpdateAsync(Brand brand);
        Task DeleteAsync(Brand brand);
        Task<bool> ExistsAsync(long brandId);
    }
}
