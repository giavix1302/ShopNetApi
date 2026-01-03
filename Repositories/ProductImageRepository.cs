using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;

namespace ShopNetApi.Repositories
{
    public class ProductImageRepository : IProductImageRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductImageRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(ProductImage image)
        {
            _db.ProductImages.Add(image);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductImage image)
        {
            _db.ProductImages.Update(image);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(ProductImage image)
        {
            _db.ProductImages.Remove(image);
            await _db.SaveChangesAsync();
        }

        public async Task<ProductImage?> GetByIdAsync(long id)
        {
            return await _db.ProductImages.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<ProductImage>> GetByProductIdAsync(long productId)
        {
            return await _db.ProductImages
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.IsPrimary)
                .ToListAsync();
        }

        public async Task<ProductImage?> GetPrimaryAsync(long productId)
        {
            return await _db.ProductImages
                .FirstOrDefaultAsync(x => x.ProductId == productId && x.IsPrimary);
        }

        public async Task<bool> AnyByProductAsync(long productId)
        {
            return await _db.ProductImages.AnyAsync(x => x.ProductId == productId);
        }

        public async Task UnsetPrimaryAsync(long productId)
        {
            await _db.ProductImages
                .Where(x => x.ProductId == productId && x.IsPrimary)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(x => x.IsPrimary, false));
        }
    }
}
