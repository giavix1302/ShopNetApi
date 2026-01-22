using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopNetApi.Models;

namespace ShopNetApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<long>, long>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ProductColor> ProductColors => Set<ProductColor>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Color> Colors => Set<Color>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<OrderTracking> OrderTrackings => Set<OrderTracking>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();
        public DbSet<Review> Reviews => Set<Review>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<UserRole>();
            modelBuilder.HasPostgresEnum<OrderStatus>();

            // ========= COLOR CONSTRAINT =========
            modelBuilder.Entity<Color>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ColorName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.HexCode)
                    .HasMaxLength(7);

                entity.HasIndex(x => x.ColorName)
                    .IsUnique(); // ❌ cấm trùng tên màu
            });

            modelBuilder.Entity<ProductColor>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => new { x.ProductId, x.ColorId })
                    .IsUnique(); // ❌ cấm trùng Color trong 1 Product

                entity.HasOne(x => x.Product)
                    .WithMany(p => p.ProductColors)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Color)
                    .WithMany(c => c.ProductColors)
                    .HasForeignKey(x => x.ColorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ProductImage>()
                .HasIndex(x => new { x.ProductId, x.IsPrimary })
                .IsUnique()
                .HasFilter("\"IsPrimary\" = true");

            modelBuilder.Entity<ProductSpecification>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.SpecName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.SpecValue)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasIndex(x => new { x.ProductId, x.SpecName })
                    .IsUnique(); // ❌ cấm trùng SpecName trong 1 Product

                entity.HasOne(x => x.Product)
                    .WithMany(p => p.Specifications)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========= CART CONSTRAINT =========
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.UserId)
                    .IsUnique(); // Mỗi user chỉ có 1 cart

                entity.HasOne(x => x.User)
                    .WithOne(u => u.Cart)
                    .HasForeignKey<Cart>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========= CART ITEM CONSTRAINT =========
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.UnitPrice)
                    .IsRequired(); // Giá bắt buộc phải có

                entity.Property(x => x.Quantity)
                    .IsRequired();

                // Cấm trùng sản phẩm cùng màu trong 1 cart
                // Nếu muốn thêm lại, sẽ cộng dồn Quantity thay vì tạo mới
                // Lưu ý: Với ColorId nullable, PostgreSQL cho phép nhiều NULL values
                // Nếu cần strict unique (kể cả NULL), cần dùng partial index hoặc xử lý trong application logic
                entity.HasIndex(x => new { x.CartId, x.ProductId, x.ColorId })
                    .IsUnique();

                entity.HasOne(x => x.Cart)
                    .WithMany(c => c.Items)
                    .HasForeignKey(x => x.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Color)
                    .WithMany()
                    .HasForeignKey(x => x.ColorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
