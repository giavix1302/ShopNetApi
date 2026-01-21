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
        private readonly ICloudinaryService _cloudinary;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ProductImageService> _logger;
        private readonly ICurrentUserService _currentUser;

        public ProductImageService(
            IProductImageRepository repo,
            ICloudinaryService cloudinary,
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

            // Check có ảnh nào và có primary không TRƯỚC khi upload (tránh race condition)
            var hasAnyImage = await _repo.AnyByProductAsync(dto.ProductId);
            var existingPrimary = await _repo.GetPrimaryAsync(dto.ProductId);

            // Upload ảnh lên Cloudinary
            var upload = await _cloudinary.UploadImageAsync(dto.Image);

            bool isPrimary;

            // 🔥 Rule: ảnh đầu tiên luôn là primary
            if (!hasAnyImage)
            {
                isPrimary = true;
            }
            // Nếu không có primary (edge case: primary bị xóa nhưng còn ảnh khác)
            else if (existingPrimary == null)
            {
                isPrimary = true;
            }
            // Nếu có primary và muốn set primary mới
            else if (dto.IsPrimary)
            {
                await _repo.UnsetPrimaryAsync(dto.ProductId);
                isPrimary = true;
            }
            else
            {
                isPrimary = false;
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
                "Product image created. ProductId={ProductId}, ImageId={ImageId}, IsPrimary={IsPrimary} | Email={Email}",
                image.ProductId, image.Id, image.IsPrimary, _currentUser.Email);

            return Map(image);
        }


        // ================= UPDATE =================
        public async Task<ProductImageResponseDto> UpdateAsync(long productId, long imageId, UpdateProductImageDto dto)
        {
            var image = await _repo.GetByIdAsync(imageId);
            if (image == null)
                throw new NotFoundException("Product image not found");

            // Validate image thuộc về product đúng
            if (image.ProductId != productId)
                throw new NotFoundException("Product image not found for this product");

            // Xử lý IsPrimary: chỉ xử lý nếu có truyền vào (nullable)
            if (dto.IsPrimary.HasValue)
            {
                // ❌ Không cho unset primary
                if (image.IsPrimary && dto.IsPrimary.Value == false)
                    throw new ConflictException("Cannot unset primary image");

                // Set primary mới nếu cần
                if (dto.IsPrimary.Value == true && !image.IsPrimary)
                {
                    // Unset primary cũ trước (nếu có)
                    await _repo.UnsetPrimaryAsync(image.ProductId);
                    image.IsPrimary = true;
                }
                // Nếu dto.IsPrimary == false và image không phải primary → không làm gì
            }

            // Xử lý upload ảnh mới: upload trước, nếu thành công mới xóa ảnh cũ
            if (dto.Image != null)
            {
                var upload = await _cloudinary.UploadImageAsync(dto.Image);
                // Upload thành công, giờ mới xóa ảnh cũ
                await _cloudinary.DeleteImageAsync(image.PublicId);
                image.ImageUrl = upload.Url;
                image.PublicId = upload.PublicId;
            }

            // Xử lý AltText: chỉ update nếu có truyền vào
            if (dto.AltText != null)
            {
                image.AltText = dto.AltText;
            }

            await _repo.UpdateAsync(image);

            return Map(image);
        }


        // ================= DELETE =================
        public async Task DeleteAsync(long productId, long imageId)
        {
            var image = await _repo.GetByIdAsync(imageId);
            if (image == null)
                throw new NotFoundException("Product image not found");

            // Validate image thuộc về product đúng
            if (image.ProductId != productId)
                throw new NotFoundException("Product image not found for this product");

            // Kiểm tra số lượng ảnh còn lại
            var allImages = await _repo.GetByProductIdAsync(productId);
            var isLastImage = allImages.Count == 1;

            // Nếu là primary image
            if (image.IsPrimary)
            {
                // Nếu là ảnh cuối cùng → cho phép xóa (sẽ không còn ảnh nào)
                if (isLastImage)
                {
                    // OK, có thể xóa primary image cuối cùng
                }
                else
                {
                    // Không cho xóa primary nếu còn ảnh khác
                    throw new ConflictException("Cannot delete primary image. Please set another image as primary first.");
                }
            }

            await _cloudinary.DeleteImageAsync(image.PublicId);
            await _repo.DeleteAsync(image);

            _logger.LogWarning(
                "Product image deleted. ImageId={ImageId}, WasPrimary={WasPrimary} | Email={Email}",
                imageId, image.IsPrimary, _currentUser.Email);
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
