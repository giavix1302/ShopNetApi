using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;

namespace ShopNetApi.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> ExistsBySlugAsync(string slug, long? excludeId = null)
        {
            var query = _db.Products.AsQueryable();

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync(x => x.Slug.ToLower() == slug.ToLower());
        }

        public async Task AddAsync(Product product)
        {
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
        }

        public async Task<Product?> GetByIdAsync(long id)
        {
            return await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Product?> GetByIdWithRelationsAsync(long id)
        {
            return await _db.Products
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Include(x => x.ProductColors)
                .ThenInclude(pc => pc.Color)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _db.Products
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Include(x => x.ProductColors)
                .ThenInclude(pc => pc.Color)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
