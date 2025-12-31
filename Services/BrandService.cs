

using Microsoft.EntityFrameworkCore;
using ShopNetApi.DTOs.Brand;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Services
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepo;

        public BrandService(IBrandRepository brandRepo)
        {
            _brandRepo = brandRepo;
        }

        public async Task<BrandResponseDto> CreateAsync(CreateBrandDto dto)
        {
            if (await _brandRepo.ExistsByNameAsync(dto.Name))
                throw new Exception("Brand name already exists");

            var brand = new Brand
            {
                Name = dto.Name,
                Description = dto.Description
            };

            await _brandRepo.AddAsync(brand);

            return MapToResponse(brand);
        }

        // ================= UPDATE =================
        public async Task<BrandResponseDto> UpdateAsync(long id, UpdateBrandDto dto)
        {
            var brand = await _brandRepo.GetByIdAsync(id);
            if (brand == null)
                throw new Exception("Brand not found");

            if (await _brandRepo.ExistsByNameAsync(dto.Name, id))
                throw new Exception("Brand name already exists");

            brand.Name = dto.Name;
            brand.Description = dto.Description;

            await _brandRepo.UpdateAsync(brand);

            return MapToResponse(brand);
        }

        // ================= DELETE =================
        public async Task DeleteAsync(long id)
        {
            var brand = await _brandRepo.GetByIdWithProductsAsync(id);
            if (brand == null)
                throw new Exception("Brand not found");

            if (brand.Products.Any())
                throw new Exception("Cannot delete brand with existing products");

            await _brandRepo.DeleteAsync(brand);
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
                throw new Exception("Brand not found");

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
