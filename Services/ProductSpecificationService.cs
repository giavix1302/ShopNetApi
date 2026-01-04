using ShopNetApi.DTOs.ProductSpecification;
using ShopNetApi.Exceptions;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Services
{
    public class ProductSpecificationService : IProductSpecificationService
    {
        private readonly IProductSpecificationRepository _repo;
        private readonly IProductRepository _productRepo;
        private readonly ILogger<ProductSpecificationService> _logger;
        private readonly ICurrentUserService _currentUser;

        public ProductSpecificationService(
            IProductSpecificationRepository repo,
            IProductRepository productRepo,
            ILogger<ProductSpecificationService> logger,
            ICurrentUserService currentUser)
        {
            _repo = repo;
            _productRepo = productRepo;
            _logger = logger;
            _currentUser = currentUser;
        }

        // ================= CREATE =================
        public async Task<ProductSpecificationResponseDto> CreateAsync(
            long productId,
            CreateProductSpecificationDto dto)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null)
                throw new NotFoundException("Product not found");

            if (await _repo.ExistsAsync(productId, dto.SpecName))
                throw new ConflictException("Specification already exists");

            var spec = new ProductSpecification
            {
                ProductId = productId,
                SpecName = dto.SpecName,
                SpecValue = dto.SpecValue
            };

            await _repo.AddAsync(spec);

            _logger.LogInformation(
                "ProductSpecification created. SpecId={SpecId}, ProductId={ProductId}, Name={SpecName} | Email={Email}",
                spec.Id, productId, spec.SpecName, _currentUser.Email);

            return MapToResponse(spec);
        }

        // ================= GET BY PRODUCT =================
        public async Task<List<ProductSpecificationResponseDto>> GetByProductIdAsync(
            long productId)
        {
            var specs = await _repo.GetByProductIdAsync(productId);

            _logger.LogInformation(
                "Get ProductSpecifications. ProductId={ProductId}, Count={Count} | Email={Email}",
                productId, specs.Count, _currentUser.Email);

            return specs.Select(MapToResponse).ToList();
        }

        // ================= UPDATE =================
        public async Task<ProductSpecificationResponseDto> UpdateAsync(
            long id,
            UpdateProductSpecificationDto dto)
        {
            var spec = await _repo.GetByIdAsync(id);
            if (spec == null)
                throw new NotFoundException("Specification not found");

            if (await _repo.ExistsAsync(spec.ProductId, dto.SpecName, id))
                throw new ConflictException("Specification already exists");

            spec.SpecName = dto.SpecName;
            spec.SpecValue = dto.SpecValue;

            await _repo.UpdateAsync(spec);

            _logger.LogInformation(
                "ProductSpecification updated. SpecId={SpecId}, ProductId={ProductId}, Name={SpecName} | Email={Email}",
                spec.Id, spec.ProductId, spec.SpecName, _currentUser.Email);

            return MapToResponse(spec);
        }

        // ================= DELETE =================
        public async Task DeleteAsync(long id)
        {
            var spec = await _repo.GetByIdAsync(id);
            if (spec == null)
                throw new NotFoundException("Specification not found");

            await _repo.DeleteAsync(spec);

            _logger.LogWarning(
                "ProductSpecification deleted. SpecId={SpecId}, ProductId={ProductId} | Email={Email}",
                spec.Id, spec.ProductId, _currentUser.Email);
        }

        // ================= MAPPER =================
        private static ProductSpecificationResponseDto MapToResponse(
            ProductSpecification spec)
        {
            return new ProductSpecificationResponseDto
            {
                Id = spec.Id,
                SpecName = spec.SpecName,
                SpecValue = spec.SpecValue
            };
        }
    }

}
