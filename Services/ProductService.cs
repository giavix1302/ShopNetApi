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
        private readonly IBrandRepository _brandRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IColorRepository _colorRepo;

        public ProductService(
            IProductRepository productRepo,
            ILogger<ProductService> logger,
            ICurrentUserService currentUser,
            IBrandRepository brandRepo,
            ICategoryRepository categoryRepo,
            IColorRepository colorRepo)
        {
            _productRepo = productRepo;
            _logger = logger;
            _currentUser = currentUser;
            _brandRepo = brandRepo;
            _categoryRepo = categoryRepo;
            _colorRepo = colorRepo;
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

            if (!await _brandRepo.ExistsAsync(dto.BrandId))
                throw new BadRequestException("Brand không tồn tại");

            if (!await _categoryRepo.ExistsAsync(dto.CategoryId))
                throw new BadRequestException("Category không tồn tại");

            if (dto.ColorIds == null || !dto.ColorIds.Any())
                throw new BadRequestException("Phải chọn ít nhất 1 màu");

            var colorsExist = await _colorRepo.AllExistAsync(dto.ColorIds);
            if (!colorsExist)
                throw new BadRequestException("Một hoặc nhiều màu không tồn tại");

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

            if (dto.Specifications != null && dto.Specifications.Any())
            {
                product.Specifications = dto.Specifications.Select(s => new ProductSpecification
                {
                    SpecName = s.Key,
                    SpecValue = s.Value
                }).ToList();
            }

            if (dto.ColorIds != null && dto.ColorIds.Any())
            {
                product.ProductColors = dto.ColorIds.Select(colorId =>
                    new ProductColor
                    {
                        ColorId = colorId
                    }).ToList();
            }


            await _productRepo.AddAsync(product);

            _logger.LogInformation(
                "Product created. ProductId={ProductId}, Slug={Slug} | Email={Email}",
                product.Id, product.Slug, _currentUser.Email);

            var createdProduct = await _productRepo.GetByIdWithIncludesAsync(product.Id)
                ?? throw new NotFoundException("Product vừa tạo không tồn tại");

            return MapToResponse(product);
        }


        // ================= UPDATE =================
        public async Task<ProductResponseDto> UpdateAsync(long id, UpdateProductDto dto)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("Product not found");

            var hasSpecsInDb = await _productRepo.HasSpecificationsAsync(product.Id);

            if (hasSpecsInDb)
            {
                if (dto.Specifications == null)
                    throw new BadRequestException(
                        "Specifications must be provided when product already has specifications"
                    );

                var newSpecs = dto.Specifications.Select(s => new ProductSpecification
                {
                    ProductId = product.Id,
                    SpecName = s.Key,
                    SpecValue = s.Value
                }).ToList();

                await _productRepo.ReplaceSpecificationsAsync(product, newSpecs);
            }
            else
            {
                if (dto.Specifications != null && dto.Specifications.Any())
                {
                    product.Specifications = dto.Specifications.Select(s => new ProductSpecification
                    {
                        SpecName = s.Key,
                        SpecValue = s.Value
                    }).ToList();
                }
            }

            if (dto.ColorIds != null)
            {
                await _productRepo.ReplaceColorsAsync(product, dto.ColorIds);
            }

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

            _logger.LogInformation(
                "Product updated. ProductId={ProductId} | Email={Email}",
                product.Id,
                _currentUser.Email
            );

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
