using Microsoft.AspNetCore.Identity;

namespace ShopNetApi.Models
{
    public class ApplicationUser : IdentityUser<long>
    {
        // ===== Thông tin mở rộng =====
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
        public string? AvatarUrl { get; set; }

        // ===== Trạng thái =====
        public bool Enabled { get; set; } = true;

        // ===== Audit =====
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ===== Navigation =====
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public Cart? Cart { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
