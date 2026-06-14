using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Icon { get; set; } // emoji or icon class

        public int? ParentId { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey("ParentId")]
        public Category? Parent { get; set; }
        public ICollection<Category> Children { get; set; } = new List<Category>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Gender { get; set; } = "unisex"; // men, women, unisex, kids

        [MaxLength(50)]
        public string? Material { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal BasePrice { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal RatingAverage { get; set; } = 0;

        public int RatingCount { get; set; } = 0;

        public bool IsFeatured { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = null!;
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }

    public class ProductImage
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }

        [Required, MaxLength(500)]
        public string Url { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 0;

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
    }

    public class ProductVariant
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }

        [Required, MaxLength(50)]
        public string Sku { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Color { get; set; }

        [MaxLength(10)]
        public string? ColorHex { get; set; }

        [MaxLength(10)]
        public string? Size { get; set; } // S, M, L, XL, 42, 9UK

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Mrp { get; set; } // original MRP

        public int Stock { get; set; } = 0;

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
    }

    public class Review
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }

        public int Rating { get; set; } // 1-5

        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(2000)]
        public string? Comment { get; set; }

        public bool IsVerifiedPurchase { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
