using ShopNetApi.DTOs.User.Admin;
using ShopNetApi.Models;

namespace ShopNetApi.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByIdAsync(long id);
        Task<ApplicationUser?> GetByIdWithDetailsAsync(long id);
        Task<(List<ApplicationUser> Items, int TotalItems)> GetAdminListAsync(AdminUserQueryDto query);
        Task<int> GetOrderCountByUserIdAsync(long userId);
        Task<int> GetReviewCountByUserIdAsync(long userId);
    }
}
