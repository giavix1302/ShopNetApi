using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;

namespace ShopNetApi.Repositories
{
    public class ProductSpecificationRepository : IProductSpecificationRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductSpecificationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> ExistsAsync(long productId, string specName, long? excludeId = null)
        {
            var query = _db.ProductSpecifications
                .Where(x => x.ProductId == productId &&
                            x.SpecName.ToLower() == specName.ToLower());

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task AddAsync(ProductSpecification spec)
        {
            _db.ProductSpecifications.Add(spec);
            await _db.SaveChangesAsync();
        }

        public async Task<ProductSpecification?> GetByIdAsync(long id)
        {
            return await _db.ProductSpecifications.FindAsync(id);
        }

        public async Task<List<ProductSpecification>> GetByProductIdAsync(long productId)
        {
            return await _db.ProductSpecifications
                .Where(x => x.ProductId == productId)
                .ToListAsync();
        }

        public async Task UpdateAsync(ProductSpecification spec)
        {
            _db.ProductSpecifications.Update(spec);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(ProductSpecification spec)
        {
            _db.ProductSpecifications.Remove(spec);
            await _db.SaveChangesAsync();
        }
    }
}
