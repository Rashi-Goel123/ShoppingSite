using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Email { get; set; }

        public string? PasswordHash { get; set; } // For admin accounts

        [MaxLength(500)]
        public string? Avatar { get; set; }

        [Required, MaxLength(20)]
        public string Role { get; set; } = "user"; // "user" | "admin"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }

    public class Address
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [MaxLength(50)]
        public string Label { get; set; } = "Home"; // "Home", "Work", "Other"

        [MaxLength(200)]
        public string Street { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Pincode { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;

        // Navigation
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }

    public class WishlistItem
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
    }

    public class OtpRecord
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required, MaxLength(6)]
        public string Code { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
