using ShopNetApi.DTOs.Product;
using ShopNetApi.Exceptions;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;
using ShopNetApi.Services.Interfaces;
using ShopNetApi.Utils;

namespace ShopNetApi.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly ILogger<ProductService> _logger;
        private readonly ICurrentUserService _currentUser;

        public ProductService(
            IProductRepository productRepo,
            ILogger<ProductService> logger,
            ICurrentUserService currentUser)
        {
            _productRepo = productRepo;
            _logger = logger;
            _currentUser = currentUser;
        }

        // ================= CREATE =================
        public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
        {
            var baseSlug = SlugHelper.GenerateSlug(dto.Name);
            var slug = baseSlug;
            var count = 1;

            while (await _productRepo.ExistsBySlugAsync(slug))
            {
                slug = $"{baseSlug}-{count++}";
            }

            var product = new Product
            {
                Name = dto.Name,
                Slug = slug,
                Description = dto.Description,
                Price = dto.Price,
                DiscountPrice = dto.DiscountPrice,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId
            };

            await _productRepo.AddAsync(product);

            _logger.LogInformation(
                "Product created. ProductId={ProductId}, Slug={Slug} | Email={Email}",
                product.Id, product.Slug, _currentUser.Email);

            return MapToResponse(product);
        }


        // ================= UPDATE =================
        public async Task<ProductResponseDto> UpdateAsync(long id, UpdateProductDto dto)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("Product not found");

            if (product.Name != dto.Name)
            {
                var baseSlug = SlugHelper.GenerateSlug(dto.Name);
                var slug = baseSlug;
                var count = 1;

                while (await _productRepo.ExistsBySlugAsync(slug, id))
                {
                    slug = $"{baseSlug}-{count++}";
                }

                product.Slug = slug;
            }

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.DiscountPrice = dto.DiscountPrice;
            product.StockQuantity = dto.StockQuantity;
            product.IsActive = dto.IsActive;
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;

            await _productRepo.UpdateAsync(product);

            return MapToResponse(product);
        }


        // ================= DELETE =================
        public async Task DeleteAsync(long id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("Product not found");

            await _productRepo.DeleteAsync(product);

            _logger.LogWarning(
                "Product deleted. ProductId={ProductId}, Name={Name} | Email={Email}",
                product.Id, product.Name, _currentUser.Email);
        }

        // ================= GET =================
        public async Task<List<ProductResponseDto>> GetAllAsync()
        {
            var products = await _productRepo.GetAllAsync();
            return products.Select(MapToResponse).ToList();
        }

        public async Task<ProductResponseDto> GetByIdAsync(long id)
        {
            var product = await _productRepo.GetByIdWithRelationsAsync(id);
            if (product == null)
                throw new NotFoundException("Product not found");

            return MapToResponse(product);
        }

        // ================= MAPPER =================
        private static ProductResponseDto MapToResponse(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name,
                CreatedAt = product.CreatedAt,

                Colors = product.ProductColors.Select(pc => new ProductColorResponseDto
                {
                    Id = pc.Color.Id,
                    ColorName = pc.Color.ColorName,
                    HexCode = pc.Color.HexCode
                }).ToList()
            };
        }

    }
}
