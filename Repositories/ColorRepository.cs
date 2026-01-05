using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;

namespace ShopNetApi.Repositories
{
    public class ColorRepository : IColorRepository
    {
        private readonly ApplicationDbContext _db;

        public ColorRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> ExistsByNameAsync(string colorName, long? excludeId = null)
        {
            var query = _db.Colors.AsQueryable();

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync(
                x => x.ColorName.ToLower() == colorName.ToLower()
            );
        }

        public async Task AddAsync(Color color)
        {
            _db.Colors.Add(color);
            await _db.SaveChangesAsync();
        }

        public async Task<Color?> GetByIdAsync(long id)
        {
            return await _db.Colors
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Color?> GetByIdWithProductColorsAsync(long id)
        {
            return await _db.Colors
                .Include(x => x.ProductColors)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Color>> GetAllAsync()
        {
            return await _db.Colors
                .OrderBy(x => x.ColorName)
                .ToListAsync();
        }

        public async Task UpdateAsync(Color color)
        {
            _db.Colors.Update(color);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Color color)
        {
            _db.Colors.Remove(color);
            await _db.SaveChangesAsync();
        }
    }
}
