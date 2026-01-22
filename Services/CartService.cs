using ShopNetApi.DTOs.Cart;
using ShopNetApi.Exceptions;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepo;
        private readonly IProductRepository _productRepo;
        private readonly IColorRepository _colorRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<CartService> _logger;

        public CartService(
            ICartRepository cartRepo,
            IProductRepository productRepo,
            IColorRepository colorRepo,
            ICurrentUserService currentUser,
            ILogger<CartService> logger)
        {
            _cartRepo = cartRepo;
            _productRepo = productRepo;
            _colorRepo = colorRepo;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<CartResponseDto> GetMyCartAsync()
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("User chưa đăng nhập");

            var cart = await _cartRepo.GetByUserIdWithItemsAsync(userId.Value);
            if (cart == null)
            {
                // Tạo cart trống nếu chưa có
                cart = new Cart
                {
                    UserId = userId.Value,
                    CreatedAt = DateTime.UtcNow
                };
                await _cartRepo.AddAsync(cart);
                // Load lại để có items collection
                cart = await _cartRepo.GetByUserIdWithItemsAsync(userId.Value);
            }

            return MapToResponse(cart!);
        }

        public async Task<CartItemResponseDto> AddItemAsync(AddCartItemDto dto)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("User chưa đăng nhập");

            // Validate product
            var product = await _productRepo.GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new NotFoundException("Sản phẩm không tồn tại");

            if (!product.IsActive)
                throw new BadRequestException("Sản phẩm không còn hoạt động");

            if (dto.Quantity > product.StockQuantity)
                throw new BadRequestException($"Số lượng không đủ. Còn {product.StockQuantity} sản phẩm");

            // Validate color nếu có
            if (dto.ColorId.HasValue)
            {
                var color = await _colorRepo.GetByIdAsync(dto.ColorId.Value);
                if (color == null)
                    throw new NotFoundException("Màu không tồn tại");

                // Kiểm tra color có thuộc product không
                var productHasColor = await _productRepo.GetByIdWithIncludesAsync(dto.ProductId);
                if (productHasColor == null || !productHasColor.ProductColors.Any(pc => pc.ColorId == dto.ColorId.Value))
                    throw new BadRequestException("Màu này không thuộc sản phẩm");
            }

            // Lấy hoặc tạo cart
            var cart = await _cartRepo.GetByUserIdAsync(userId.Value);
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId.Value,
                    CreatedAt = DateTime.UtcNow
                };
                await _cartRepo.AddAsync(cart);
            }

            // Kiểm tra item đã tồn tại chưa
            var existingItem = await _cartRepo.GetItemByCartIdAndProductIdAndColorIdAsync(
                cart.Id, dto.ProductId, dto.ColorId);

            decimal unitPrice = product.DiscountPrice ?? product.Price;

            if (existingItem != null)
            {
                // Cộng dồn quantity
                var newQuantity = existingItem.Quantity + dto.Quantity;
                if (newQuantity > product.StockQuantity)
                    throw new BadRequestException($"Tổng số lượng không được vượt quá {product.StockQuantity}");

                existingItem.Quantity = newQuantity;
                existingItem.UnitPrice = unitPrice;
                await _cartRepo.UpdateItemAsync(existingItem);

                // Load lại với Product và Color
                var updatedItem = await _cartRepo.GetItemsByCartIdWithProductAsync(cart.Id);
                var itemWithData = updatedItem.FirstOrDefault(i => i.Id == existingItem.Id);
                if (itemWithData != null)
                {
                    _logger.LogInformation(
                        "Cart item quantity updated. ItemId={ItemId}, NewQuantity={Quantity} | UserId={UserId}",
                        existingItem.Id, newQuantity, userId.Value);

                    return MapItemToResponse(itemWithData, product);
                }

                return MapItemToResponse(existingItem, product);
            }
            else
            {
                // Tạo item mới
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    ColorId = dto.ColorId,
                    Quantity = dto.Quantity,
                    UnitPrice = unitPrice
                };

                await _cartRepo.AddItemAsync(newItem);

                // Load lại với Product và Color
                var items = await _cartRepo.GetItemsByCartIdWithProductAsync(cart.Id);
                var itemWithData = items.FirstOrDefault(i => 
                    i.ProductId == dto.ProductId && 
                    i.ColorId == dto.ColorId);

                _logger.LogInformation(
                    "Cart item added. ItemId={ItemId}, ProductId={ProductId}, Quantity={Quantity} | UserId={UserId}",
                    newItem.Id, dto.ProductId, dto.Quantity, userId.Value);

                if (itemWithData != null)
                    return MapItemToResponse(itemWithData, product);

                return MapItemToResponse(newItem, product);
            }
        }

        public async Task<CartItemResponseDto> UpdateItemAsync(long itemId, UpdateCartItemDto dto)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("User chưa đăng nhập");

            var item = await _cartRepo.GetItemByIdAsync(itemId);
            if (item == null)
                throw new NotFoundException("Cart item không tồn tại");

            // Kiểm tra item thuộc cart của user hiện tại
            var cart = await _cartRepo.GetByIdAsync(item.CartId);
            if (cart == null || cart.UserId != userId.Value)
                throw new ForbiddenException("Không có quyền truy cập cart item này");

            // Validate product
            var product = await _productRepo.GetByIdAsync(item.ProductId);
            if (product == null)
                throw new NotFoundException("Sản phẩm không tồn tại");

            if (!product.IsActive)
                throw new BadRequestException("Sản phẩm không còn hoạt động");

            if (dto.Quantity > product.StockQuantity)
                throw new BadRequestException($"Số lượng không đủ. Còn {product.StockQuantity} sản phẩm");

            // Cập nhật quantity và price
            item.Quantity = dto.Quantity;
            item.UnitPrice = product.DiscountPrice ?? product.Price;
            await _cartRepo.UpdateItemAsync(item);

            // Load lại với Product và Color
            var items = await _cartRepo.GetItemsByCartIdWithProductAsync(cart.Id);
            var itemWithData = items.FirstOrDefault(i => i.Id == itemId);

            _logger.LogInformation(
                "Cart item updated. ItemId={ItemId}, Quantity={Quantity} | UserId={UserId}",
                itemId, dto.Quantity, userId.Value);

            if (itemWithData != null)
                return MapItemToResponse(itemWithData, product);

            return MapItemToResponse(item, product);
        }

        public async Task DeleteItemAsync(long itemId)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("User chưa đăng nhập");

            var item = await _cartRepo.GetItemByIdAsync(itemId);
            if (item == null)
                throw new NotFoundException("Cart item không tồn tại");

            // Kiểm tra item thuộc cart của user hiện tại
            var cart = await _cartRepo.GetByIdAsync(item.CartId);
            if (cart == null || cart.UserId != userId.Value)
                throw new ForbiddenException("Không có quyền xóa cart item này");

            await _cartRepo.DeleteItemAsync(item);

            _logger.LogInformation(
                "Cart item deleted. ItemId={ItemId} | UserId={UserId}",
                itemId, userId.Value);
        }

        public async Task ClearCartAsync()
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("User chưa đăng nhập");

            var cart = await _cartRepo.GetByUserIdAsync(userId.Value);
            if (cart == null)
                return; // Cart không tồn tại thì không cần xóa

            // Xóa tất cả items
            await _cartRepo.DeleteItemsByCartIdAsync(cart.Id);
            // Xóa cart
            await _cartRepo.DeleteAsync(cart);

            _logger.LogInformation(
                "Cart cleared. CartId={CartId} | UserId={UserId}",
                cart.Id, userId.Value);
        }

        // ========= MAPPING METHODS =========

        private CartResponseDto MapToResponse(Cart cart)
        {
            var items = cart.Items.Select(item =>
            {
                var product = item.Product;
                return new CartItemResponseDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = product?.Name ?? "",
                    ProductSlug = product?.Slug,
                    ColorId = item.ColorId,
                    ColorName = item.Color?.ColorName,
                    ColorHexCode = item.Color?.HexCode,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice
                };
            }).ToList();

            return new CartResponseDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CreatedAt = cart.CreatedAt,
                Items = items,
                TotalAmount = items.Sum(i => i.TotalPrice),
                TotalItems = items.Sum(i => i.Quantity)
            };
        }

        private CartItemResponseDto MapItemToResponse(CartItem item, Product product)
        {
            // Load Color nếu cần
            var color = item.Color;

            return new CartItemResponseDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = product.Name,
                ProductSlug = product.Slug,
                ColorId = item.ColorId,
                ColorName = color?.ColorName,
                ColorHexCode = color?.HexCode,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.Quantity * item.UnitPrice
            };
        }
    }
}
