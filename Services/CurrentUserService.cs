using ShopNetApi.Services.Interfaces;
using System.Security.Claims;

namespace ShopNetApi.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public long? UserId =>
        long.TryParse(
            _httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var id)
        ? id
        : null;

        public string? Email =>
            _httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.Email);

        public string? Name =>
            _httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.Name);
    }
}
