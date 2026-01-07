using ShopNetApi.Models;

namespace ShopNetApi.Repositories.Interfaces
{
    public interface IColorRepository
    {
        Task<bool> AllExistAsync(List<long> colorIds);
        Task<bool> ExistsByNameAsync(string colorName, long? excludeId = null);
        Task AddAsync(Color color);
        Task<Color?> GetByIdAsync(long id);
        Task<Color?> GetByIdWithProductColorsAsync(long id);
        Task<List<Color>> GetAllAsync();
        Task UpdateAsync(Color color);
        Task DeleteAsync(Color color);
    }
}
