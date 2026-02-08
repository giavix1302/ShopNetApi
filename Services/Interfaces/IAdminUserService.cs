using ShopNetApi.DTOs.User.Admin;

namespace ShopNetApi.Services.Interfaces
{
    public interface IAdminUserService
    {
        Task<AdminUserListResponseDto> GetUsersAsync(AdminUserQueryDto query);
        Task<AdminUserDetailDto> GetUserByIdAsync(long userId);
    }
}
