using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;

namespace ShopNetApi.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> ExistsByNameAsync(string name, long? excludeId = null)
        {
            var query = _db.Categories.AsQueryable();

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync(
                x => x.Name.ToLower() == name.ToLower()
            );
        }

        public async Task<Category?> GetByIdAsync(long id)
        {
            return await _db.Categories
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Category?> GetByIdWithProductsAsync(long id)
        {
            return await _db.Categories
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _db.Categories
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Category category)
        {
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _db.Categories.Update(category);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Category category)
        {
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(long categoryId)
        {
            return await _db.Categories.AnyAsync(x => x.Id == categoryId);
        }
    }
}
