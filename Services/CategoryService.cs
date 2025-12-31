using Microsoft.EntityFrameworkCore;
using ShopNetApi.DTOs.Category;
using ShopNetApi.Exceptions;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepo;

        public CategoryService(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        // ================= CREATE =================
        public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto)
        {
            if (await _categoryRepo.ExistsByNameAsync(dto.Name))
                throw new BadRequestException("Category name already exists");

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            await _categoryRepo.AddAsync(category);

            return MapToResponse(category);
        }

        // ================= UPDATE =================
        public async Task<CategoryResponseDto> UpdateAsync(long id, UpdateCategoryDto dto)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null)
                throw new NotFoundException("Category not found");

            if (await _categoryRepo.ExistsByNameAsync(dto.Name, id))
                throw new BadRequestException("Category name already exists");

            category.Name = dto.Name;
            category.Description = dto.Description;

            await _categoryRepo.UpdateAsync(category);

            return MapToResponse(category);
        }

        // ================= GET ALL =================
        public async Task<List<CategoryResponseDto>> GetAllAsync()
        {
            var categories = await _categoryRepo.GetAllAsync();

            return categories
                .Select(MapToResponse)
                .ToList();
        }

        // ================= GET BY ID =================
        public async Task<CategoryResponseDto> GetByIdAsync(long id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null)
                throw new NotFoundException("Category not found");

            return MapToResponse(category);
        }

        // ================= DELETE =================
        public async Task DeleteAsync(long id)
        {
            var category = await _categoryRepo.GetByIdWithProductsAsync(id);
            if (category == null)
                throw new NotFoundException("Category not found");

            if (category.Products.Any())
                throw new BadRequestException(
                    "Cannot delete category with existing products"
                );

            await _categoryRepo.DeleteAsync(category);
        }

        // ================= MAPPER =================
        private static CategoryResponseDto MapToResponse(Category category)
        {
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt
            };
        }

    }
}
