using ShopNetApi.Models;

namespace ShopNetApi.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<bool> ExistsByNameAsync(string name, long? excludeId = null);
        Task<Category?> GetByIdAsync(long id);
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdWithProductsAsync(long id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(Category category);
    }
}
