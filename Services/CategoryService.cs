using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.DTOs.Category;
using ShopNetApi.Models;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _db;

        public CategoryService(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto)
        {
            // Check if category with the same name already exists
            var existingCategory = await _db.Categories
                .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower());

            if (existingCategory)
                throw new Exception("Category name already exists");

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt
            };
        }

        public async Task<CategoryResponseDto> UpdateAsync(long id, UpdateCategoryDto dto)
        {
            var category = await _db.Categories
                .FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
                throw new Exception("Category not found");

            // Check trùng tên (ngoại trừ chính nó)
            var nameExists = await _db.Categories.AnyAsync(x =>
                x.Id != id &&
                x.Name.ToLower() == dto.Name.ToLower());

            if (nameExists)
                throw new Exception("Category name already exists");

            category.Name = dto.Name;
            category.Description = dto.Description;

            await _db.SaveChangesAsync();

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt
            };
        }

        public async Task<List<CategoryResponseDto>> GetAllAsync()
        {
            return await _db.Categories
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new CategoryResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<CategoryResponseDto> GetByIdAsync(long id)
        {
            var category = await _db.Categories
                .FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
                throw new Exception("Category not found");

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt
            };
        }

        public async Task DeleteAsync(long id)
        {
            var category = await _db.Categories
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
                throw new Exception("Category not found");

            if (category.Products.Any())
                throw new Exception("Cannot delete category with existing products");

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
        }

    }
}
