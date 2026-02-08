using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.DTOs.User.Admin;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;

namespace ShopNetApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<ApplicationUser?> GetByIdAsync(long id)
        {
            return await _db.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<ApplicationUser?> GetByIdWithDetailsAsync(long id)
        {
            return await _db.Users
                .Include(u => u.Orders)
                    .ThenInclude(o => o.Items)
                        .ThenInclude(i => i.Product)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.Items)
                        .ThenInclude(i => i.Color)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.Trackings)
                .Include(u => u.Reviews)
                    .ThenInclude(r => r.Product)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<(List<ApplicationUser> Items, int TotalItems)> GetAdminListAsync(AdminUserQueryDto query)
        {
            var q = _db.Users
                .AsNoTracking()
                .AsQueryable();

            // Filter by email
            if (!string.IsNullOrWhiteSpace(query.Email))
            {
                q = q.Where(u => u.Email != null && 
                    u.Email.ToLower().Contains(query.Email.Trim().ToLower()));
            }

            // Filter by fullName
            if (!string.IsNullOrWhiteSpace(query.FullName))
            {
                q = q.Where(u => u.FullName != null && 
                    u.FullName.ToLower().Contains(query.FullName.Trim().ToLower()));
            }

            // Filter by enabled
            if (query.Enabled.HasValue)
            {
                q = q.Where(u => u.Enabled == query.Enabled.Value);
            }

            // Filter by date range
            if (query.From.HasValue)
            {
                q = q.Where(u => u.CreatedAt >= query.From.Value);
            }

            if (query.To.HasValue)
            {
                q = q.Where(u => u.CreatedAt <= query.To.Value);
            }

            // Sorting
            var sortBy = (query.SortBy ?? "createdAt").Trim().ToLower();
            var sortDir = (query.SortDir ?? "desc").Trim().ToLower();

            q = (sortBy, sortDir) switch
            {
                ("email", "asc") => q.OrderBy(u => u.Email),
                ("email", "desc") => q.OrderByDescending(u => u.Email),
                ("fullname", "asc") => q.OrderBy(u => u.FullName),
                ("fullname", "desc") => q.OrderByDescending(u => u.FullName),
                ("createdat", "asc") => q.OrderBy(u => u.CreatedAt),
                _ => q.OrderByDescending(u => u.CreatedAt)
            };

            var total = await q.CountAsync();
            var skip = (query.Page - 1) * query.PageSize;
            var items = await q.Skip(skip).Take(query.PageSize).ToListAsync();

            return (items, total);
        }

        public async Task<int> GetOrderCountByUserIdAsync(long userId)
        {
            return await _db.Orders
                .AsNoTracking()
                .CountAsync(o => o.UserId == userId);
        }

        public async Task<int> GetReviewCountByUserIdAsync(long userId)
        {
            return await _db.Reviews
                .AsNoTracking()
                .CountAsync(r => r.UserId == userId);
        }
    }
}
