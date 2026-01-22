using ShopNetApi.Models;

namespace ShopNetApi.Repositories.Interfaces
{
    public interface ICartRepository
    {
        // ========= CART OPERATIONS =========
        Task<Cart?> GetByIdAsync(long id);
        Task<Cart?> GetByUserIdAsync(long userId);
        Task<Cart?> GetByUserIdWithItemsAsync(long userId);
        Task<Cart?> GetByIdWithItemsAsync(long id);
        Task<bool> ExistsByUserIdAsync(long userId);
        Task AddAsync(Cart cart);
        Task UpdateAsync(Cart cart);
        Task DeleteAsync(Cart cart);

        // ========= CART ITEM OPERATIONS =========
        Task<CartItem?> GetItemByIdAsync(long id);
        Task<CartItem?> GetItemByCartIdAndProductIdAndColorIdAsync(long cartId, long productId, long? colorId);
        Task<List<CartItem>> GetItemsByCartIdAsync(long cartId);
        Task<List<CartItem>> GetItemsByCartIdWithProductAsync(long cartId);
        Task AddItemAsync(CartItem item);
        Task UpdateItemAsync(CartItem item);
        Task DeleteItemAsync(CartItem item);
        Task DeleteItemsByCartIdAsync(long cartId);
    }
}
