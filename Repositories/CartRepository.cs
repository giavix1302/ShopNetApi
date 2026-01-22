using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;

namespace ShopNetApi.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;

        public CartRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        // ========= CART OPERATIONS =========

        public async Task<Cart?> GetByIdAsync(long id)
        {
            return await _db.Carts
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Cart?> GetByUserIdAsync(long userId)
        {
            return await _db.Carts
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<Cart?> GetByUserIdWithItemsAsync(long userId)
        {
            return await _db.Carts
                .Include(x => x.Items)
                    .ThenInclude(i => i.Product)
                .Include(x => x.Items)
                    .ThenInclude(i => i.Color)
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<Cart?> GetByIdWithItemsAsync(long id)
        {
            return await _db.Carts
                .Include(x => x.Items)
                    .ThenInclude(i => i.Product)
                .Include(x => x.Items)
                    .ThenInclude(i => i.Color)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> ExistsByUserIdAsync(long userId)
        {
            return await _db.Carts
                .AnyAsync(x => x.UserId == userId);
        }

        public async Task AddAsync(Cart cart)
        {
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Cart cart)
        {
            _db.Carts.Update(cart);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Cart cart)
        {
            _db.Carts.Remove(cart);
            await _db.SaveChangesAsync();
        }

        // ========= CART ITEM OPERATIONS =========

        public async Task<CartItem?> GetItemByIdAsync(long id)
        {
            return await _db.CartItems
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<CartItem?> GetItemByCartIdAndProductIdAndColorIdAsync(long cartId, long productId, long? colorId)
        {
            return await _db.CartItems
                .FirstOrDefaultAsync(x =>
                    x.CartId == cartId &&
                    x.ProductId == productId &&
                    x.ColorId == colorId);
        }

        public async Task<List<CartItem>> GetItemsByCartIdAsync(long cartId)
        {
            return await _db.CartItems
                .Where(x => x.CartId == cartId)
                .ToListAsync();
        }

        public async Task<List<CartItem>> GetItemsByCartIdWithProductAsync(long cartId)
        {
            return await _db.CartItems
                .Include(x => x.Product)
                .Include(x => x.Color)
                .Where(x => x.CartId == cartId)
                .ToListAsync();
        }

        public async Task AddItemAsync(CartItem item)
        {
            _db.CartItems.Add(item);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(CartItem item)
        {
            _db.CartItems.Update(item);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(CartItem item)
        {
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteItemsByCartIdAsync(long cartId)
        {
            var items = await _db.CartItems
                .Where(x => x.CartId == cartId)
                .ToListAsync();

            _db.CartItems.RemoveRange(items);
            await _db.SaveChangesAsync();
        }
    }
}
