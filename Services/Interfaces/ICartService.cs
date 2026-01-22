using ShopNetApi.DTOs.Cart;

namespace ShopNetApi.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartResponseDto> GetMyCartAsync();
        Task<CartItemResponseDto> AddItemAsync(AddCartItemDto dto);
        Task<CartItemResponseDto> UpdateItemAsync(long itemId, UpdateCartItemDto dto);
        Task DeleteItemAsync(long itemId);
        Task ClearCartAsync();
    }
}
