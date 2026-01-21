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
                .Include(x => x.Specifications)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Product?> GetByIdWithSpecificationsAsync(long id)
        {
            return await _db.Products
                .Include(x => x.Specifications)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _db.Products
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Include(x => x.ProductColors)
                .ThenInclude(pc => pc.Color)
                .Include(x => x.Specifications)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task ReplaceSpecificationsAsync(
            Product product,
            List<ProductSpecification> specs
        )
        {
            // Load existing specs (nếu chưa load)
            await _db.Entry(product)
                .Collection(p => p.Specifications)
                .LoadAsync();

            // Xóa cũ
            _db.ProductSpecifications.RemoveRange(product.Specifications);

            // Gán mới
            foreach (var spec in specs)
            {
                spec.ProductId = product.Id;
            }

            await _db.ProductSpecifications.AddRangeAsync(specs);
        }

        public async Task<bool> HasColorsAsync(long productId)
        {
            return await _db.ProductColors
                .AnyAsync(pc => pc.ProductId == productId);
        }

        public async Task ReplaceColorsAsync(Product product, List<long> colorIds)
        {
            // 1️⃣ Load colors hiện tại nếu chưa load
            await _db.Entry(product)
                .Collection(p => p.ProductColors)
                .LoadAsync();

            // 2️⃣ Xóa toàn bộ mapping cũ
            _db.ProductColors.RemoveRange(product.ProductColors);

            // 3️⃣ Thêm mapping mới (distinct để tránh trùng)
            var newColors = colorIds
                .Distinct()
                .Select(colorId => new ProductColor
                {
                    ProductId = product.Id,
                    ColorId = colorId
                })
                .ToList();

            await _db.ProductColors.AddRangeAsync(newColors);
        }

        public async Task<Product?> GetByIdWithIncludesAsync(long id)
        {
            return await _db.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductColors)
                    .ThenInclude(pc => pc.Color)
                .Include(p => p.Specifications)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

    }
}
