

using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.DTOs.Brand;
using ShopNetApi.Models;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Services
{
    public class BrandService : IBrandService
    {
        private readonly ApplicationDbContext _db;

        public BrandService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<BrandResponseDto> CreateAsync(CreateBrandDto dto)
        {
            var exists = await _db.Brands
                .AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower());

            if (exists)
                throw new Exception("Brand name already exists");

            var brand = new Brand
            {
                Name = dto.Name,
                Description = dto.Description
            };

            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();

            return new BrandResponseDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                CreatedAt = brand.CreatedAt
            };
        }

        // ================= UPDATE =================
        public async Task<BrandResponseDto> UpdateAsync(long id, UpdateBrandDto dto)
        {
            var brand = await _db.Brands.FirstOrDefaultAsync(x => x.Id == id);
            if (brand == null)
                throw new Exception("Brand not found");

            var exists = await _db.Brands.AnyAsync(x =>
                x.Id != id &&
                x.Name.ToLower() == dto.Name.ToLower());

            if (exists)
                throw new Exception("Brand name already exists");

            brand.Name = dto.Name;
            brand.Description = dto.Description;

            await _db.SaveChangesAsync();

            return MapToResponse(brand);
        }

        // ================= DELETE =================
        public async Task DeleteAsync(long id)
        {
            var brand = await _db.Brands
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (brand == null)
                throw new Exception("Brand not found");

            if (brand.Products.Any())
                throw new Exception("Cannot delete brand with existing products");

            _db.Brands.Remove(brand);
            await _db.SaveChangesAsync();
        }

        // ================= GET ALL =================
        public async Task<List<BrandResponseDto>> GetAllAsync()
        {
            return await _db.Brands
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => MapToResponse(x))
                .ToListAsync();
        }

        // ================= GET BY ID =================
        public async Task<BrandResponseDto> GetByIdAsync(long id)
        {
            var brand = await _db.Brands.FirstOrDefaultAsync(x => x.Id == id);
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
