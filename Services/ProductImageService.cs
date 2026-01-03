using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.DTOs.ProductImage;
using ShopNetApi.Exceptions;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly IProductImageRepository _repo;
        private readonly CloudinaryService _cloudinary;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ProductImageService> _logger;
        private readonly ICurrentUserService _currentUser;

        public ProductImageService(
            IProductImageRepository repo,
            CloudinaryService cloudinary,
            ApplicationDbContext db,
            ILogger<ProductImageService> logger,
            ICurrentUserService currentUser)
        {
            _repo = repo;
            _cloudinary = cloudinary;
            _db = db;
            _logger = logger;
            _currentUser = currentUser;
        }

        // ================= CREATE =================
        public async Task<ProductImageResponseDto> CreateAsync(CreateProductImageDto dto)
        {
            if (!await _db.Products.AnyAsync(x => x.Id == dto.ProductId))
                throw new NotFoundException("Product not found");

            var upload = await _cloudinary.UploadImageAsync(dto.Image);

            var hasAnyImage = await _repo.AnyByProductAsync(dto.ProductId);

            bool isPrimary = dto.IsPrimary;

            // 🔥 Rule: ảnh đầu tiên luôn là primary
            if (!hasAnyImage)
            {
                isPrimary = true;
            }
            else if (dto.IsPrimary)
            {
                await _repo.UnsetPrimaryAsync(dto.ProductId);
            }

            var image = new ProductImage
            {
                ProductId = dto.ProductId,
                ImageUrl = upload.Url,
                PublicId = upload.PublicId,
                AltText = dto.AltText,
                IsPrimary = isPrimary
            };

            await _repo.AddAsync(image);

            _logger.LogInformation(
                "Product image created. ProductId={ProductId}, ImageId={ImageId} | Email={Email}",
                image.ProductId, image.Id, _currentUser.Email);

            return Map(image);
        }


        // ================= UPDATE =================
        public async Task<ProductImageResponseDto> UpdateAsync(long id, UpdateProductImageDto dto)
        {
            var image = await _repo.GetByIdAsync(id);
            if (image == null)
                throw new NotFoundException("Product image not found");

            // ❌ Không cho unset primary
            if (image.IsPrimary && dto.IsPrimary == false)
                throw new ConflictException("Cannot unset primary image");

            if (dto.IsPrimary == true && !image.IsPrimary)
            {
                await _repo.UnsetPrimaryAsync(image.ProductId);
                image.IsPrimary = true;
            }

            if (dto.Image != null)
            {
                await _cloudinary.DeleteImageAsync(image.PublicId);
                var upload = await _cloudinary.UploadImageAsync(dto.Image);
                image.ImageUrl = upload.Url;
                image.PublicId = upload.PublicId;
            }

            image.AltText = dto.AltText ?? image.AltText;

            await _repo.UpdateAsync(image);

            return Map(image);
        }


        // ================= DELETE =================
        public async Task DeleteAsync(long id)
        {
            var image = await _repo.GetByIdAsync(id);
            if (image == null)
                throw new NotFoundException("Product image not found");

            if (image.IsPrimary)
                throw new ConflictException("Cannot delete primary image");

            await _cloudinary.DeleteImageAsync(image.PublicId);
            await _repo.DeleteAsync(image);

            _logger.LogWarning(
                "Product image deleted. ImageId={ImageId} | Email={Email}",
                id, _currentUser.Email);
        }

        // ================= GET =================
        public async Task<List<ProductImageResponseDto>> GetByProductAsync(long productId)
        {
            var images = await _repo.GetByProductIdAsync(productId);
            return images.Select(Map).ToList();
        }

        private static ProductImageResponseDto Map(ProductImage image)
        {
            return new ProductImageResponseDto
            {
                Id = image.Id,
                ProductId = image.ProductId,
                ImageUrl = image.ImageUrl,
                AltText = image.AltText,
                IsPrimary = image.IsPrimary
            };
        }
    }
}
