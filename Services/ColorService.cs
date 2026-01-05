using ShopNetApi.DTOs.Color;
using ShopNetApi.Exceptions;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Services
{
    public class ColorService : IColorService
    {
        private readonly IColorRepository _colorRepo;
        private readonly ILogger<ColorService> _logger;
        private readonly ICurrentUserService _currentUser;

        public ColorService(
            IColorRepository colorRepo,
            ILogger<ColorService> logger,
            ICurrentUserService currentUser)
        {
            _colorRepo = colorRepo;
            _logger = logger;
            _currentUser = currentUser;
        }

        // ================= CREATE =================
        public async Task<ColorResponseDto> CreateAsync(CreateColorDto dto)
        {
            if (await _colorRepo.ExistsByNameAsync(dto.ColorName))
                throw new BadRequestException("Color name already exists");

            var color = new Color
            {
                ColorName = dto.ColorName,
                HexCode = dto.HexCode
            };

            await _colorRepo.AddAsync(color);

            _logger.LogInformation(
                "Color created. ColorId={ColorId}, Name={Name} | Email={Email}",
                color.Id, color.ColorName, _currentUser.Email);

            return MapToResponse(color);
        }

        // ================= UPDATE =================
        public async Task<ColorResponseDto> UpdateAsync(long id, UpdateColorDto dto)
        {
            var color = await _colorRepo.GetByIdAsync(id);
            if (color == null)
                throw new NotFoundException("Color not found");

            if (await _colorRepo.ExistsByNameAsync(dto.ColorName, id))
                throw new BadRequestException("Color name already exists");

            color.ColorName = dto.ColorName;
            color.HexCode = dto.HexCode;

            await _colorRepo.UpdateAsync(color);

            _logger.LogInformation(
                "Color updated. ColorId={ColorId}, Name={Name} | Email={Email}",
                color.Id, color.ColorName, _currentUser.Email);

            return MapToResponse(color);
        }

        // ================= DELETE =================
        public async Task DeleteAsync(long id)
        {
            var color = await _colorRepo.GetByIdWithProductColorsAsync(id);
            if (color == null)
                throw new NotFoundException("Color not found");

            if (color.ProductColors.Any())
                throw new BadRequestException(
                    "Cannot delete color with existing products"
                );

            await _colorRepo.DeleteAsync(color);

            _logger.LogWarning(
                "Color deleted. ColorId={ColorId}, Name={Name} | Email={Email}",
                color.Id, color.ColorName, _currentUser.Email);
        }

        // ================= GET ALL =================
        public async Task<List<ColorResponseDto>> GetAllAsync()
        {
            var colors = await _colorRepo.GetAllAsync();
            return colors.Select(MapToResponse).ToList();
        }

        // ================= GET BY ID =================
        public async Task<ColorResponseDto> GetByIdAsync(long id)
        {
            var color = await _colorRepo.GetByIdAsync(id);
            if (color == null)
                throw new NotFoundException("Color not found");

            return MapToResponse(color);
        }

        // ================= MAPPER =================
        private static ColorResponseDto MapToResponse(Color color)
        {
            return new ColorResponseDto
            {
                Id = color.Id,
                ColorName = color.ColorName,
                HexCode = color.HexCode
            };
        }
    }
}
