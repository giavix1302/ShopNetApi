using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;

namespace ShopNetApi.Repositories
{
    public class BrandRepository : IBrandRepository
    {
        private readonly ApplicationDbContext _db;

        public BrandRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> ExistsByNameAsync(string name, long? excludeId = null)
        {
            var query = _db.Brands.AsQueryable();

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync(
                x => x.Name.ToLower() == name.ToLower()
            );
        }

        public async Task AddAsync(Brand brand)
        {
            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();
        }

        public async Task<Brand?> GetByIdAsync(long id)
        {
            return await _db.Brands.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Brand?> GetByIdWithProductsAsync(long id)
        {
            return await _db.Brands
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Brand>> GetAllAsync()
        {
            return await _db.Brands
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(Brand brand)
        {
            _db.Brands.Update(brand);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Brand brand)
        {
            _db.Brands.Remove(brand);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(long brandId)
        {
            return await _db.Brands.AnyAsync(b => b.Id == brandId);
        }
    }
}
