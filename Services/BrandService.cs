using ShopNetApi.DTOs.Brand;
using ShopNetApi.Exceptions;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Services
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepo;
        private readonly ILogger<BrandService> _logger;
        private readonly ICurrentUserService _currentUser;

        public BrandService(
            IBrandRepository brandRepo,
            ILogger<BrandService> logger,
            ICurrentUserService currentUser)
        {
            _brandRepo = brandRepo;
            _logger = logger;
            _currentUser = currentUser;
        }

        // ================= CREATE =================
        public async Task<BrandResponseDto> CreateAsync(CreateBrandDto dto)
        {
            if (await _brandRepo.ExistsByNameAsync(dto.Name))
                throw new BadRequestException("Brand name already exists");

            var brand = new Brand
            {
                Name = dto.Name,
                Description = dto.Description
            };

            await _brandRepo.AddAsync(brand);

            _logger.LogInformation(
                "Brand created. BrandId={BrandId}, Name={Name} | Email={Email}",
                brand.Id, brand.Name, _currentUser.Email);

            return MapToResponse(brand);
        }

        // ================= UPDATE =================
        public async Task<BrandResponseDto> UpdateAsync(long id, UpdateBrandDto dto)
        {
            var brand = await _brandRepo.GetByIdAsync(id);
            if (brand == null)
                throw new NotFoundException("Brand not found");

            if (await _brandRepo.ExistsByNameAsync(dto.Name, id))
                throw new BadRequestException("Brand name already exists");

            brand.Name = dto.Name;
            brand.Description = dto.Description;

            await _brandRepo.UpdateAsync(brand);


            _logger.LogInformation(
                "Brand updated. BrandId={BrandId}, Name={Name} | Email={Email}",
                brand.Id, brand.Name, _currentUser.Email);

            return MapToResponse(brand);
        }

        // ================= DELETE =================
        public async Task DeleteAsync(long id)
        {
            var brand = await _brandRepo.GetByIdWithProductsAsync(id);
            if (brand == null)
                throw new NotFoundException("Brand not found");

            if (brand.Products.Any())
                throw new BadRequestException(
                    "Cannot delete brand with existing products"
                );

            await _brandRepo.DeleteAsync(brand);

            _logger.LogWarning(
                "Brand deleted. BrandId={BrandId}, Name={Name} | Email={Email}",
                brand.Id, brand.Name, _currentUser.Email);
        }

        // ================= GET ALL =================
        public async Task<List<BrandResponseDto>> GetAllAsync()
        {
            var brands = await _brandRepo.GetAllAsync();
            return brands.Select(MapToResponse).ToList();
        }

        // ================= GET BY ID =================
        public async Task<BrandResponseDto> GetByIdAsync(long id)
        {
            var brand = await _brandRepo.GetByIdAsync(id);
            if (brand == null)
                throw new NotFoundException("Brand not found");

            return MapToResponse(brand);
        }

        // ================= MAPPER =================
        private static BrandResponseDto MapToResponse(Brand brand)
        {
            return new BrandResponseDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                CreatedAt = brand.CreatedAt
            };
        }
    }
}
