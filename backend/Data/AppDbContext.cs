using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;

namespace EcommerceApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<OtpRecord> OtpRecords => Set<OtpRecord>();
        public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<TrackingEvent> TrackingEvents => Set<TrackingEvent>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Coupon> Coupons => Set<Coupon>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(u => u.Phone).IsUnique();
                e.HasIndex(u => u.Email);
            });

            // Category self-referencing
            modelBuilder.Entity<Category>(e =>
            {
                e.HasIndex(c => c.Slug).IsUnique();
                e.HasOne(c => c.Parent)
                 .WithMany(c => c.Children)
                 .HasForeignKey(c => c.ParentId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Product
            modelBuilder.Entity<Product>(e =>
            {
                e.HasIndex(p => p.Slug).IsUnique();
                e.HasQueryFilter(p => !p.IsDeleted);
            });

            // ProductVariant unique SKU
            modelBuilder.Entity<ProductVariant>(e =>
            {
                e.HasIndex(v => v.Sku).IsUnique();
            });

            // WishlistItem unique per user+product
            modelBuilder.Entity<WishlistItem>(e =>
            {
                e.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
                e.HasOne(w => w.Product).WithMany().HasForeignKey(w => w.ProductId).OnDelete(DeleteBehavior.Restrict);
            });

            // CartItem
            modelBuilder.Entity<CartItem>(e =>
            {
                e.HasIndex(c => new { c.UserId, c.VariantId }).IsUnique();
                
                e.HasOne(c => c.Product)
                 .WithMany()
                 .HasForeignKey(c => c.ProductId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(c => c.Variant)
                 .WithMany()
                 .HasForeignKey(c => c.VariantId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Order
            modelBuilder.Entity<Order>(e =>
            {
                e.HasIndex(o => o.OrderNumber).IsUnique();
                e.HasIndex(o => o.UserId);
            });

            // Coupon
            modelBuilder.Entity<Coupon>(e =>
            {
                e.HasIndex(c => c.Code).IsUnique();
            });

            // Review — one per user per product
            modelBuilder.Entity<Review>(e =>
            {
                e.HasIndex(r => new { r.UserId, r.ProductId }).IsUnique();
                e.HasOne(r => r.Product).WithMany().HasForeignKey(r => r.ProductId).OnDelete(DeleteBehavior.Restrict);
            });

            // OtpRecord
            modelBuilder.Entity<OtpRecord>(e =>
            {
                e.HasIndex(o => o.Phone);
            });

            // Notification
            modelBuilder.Entity<Notification>(e =>
            {
                e.HasIndex(n => new { n.UserId, n.IsRead });
            });
        }
    }
}
