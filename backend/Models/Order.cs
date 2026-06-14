using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string OrderNumber { get; set; } = string.Empty; // ORD-20260613-XXXX

        public int UserId { get; set; }

        // Shipping address snapshot
        [MaxLength(200)]
        public string ShipStreet { get; set; } = string.Empty;
        [MaxLength(100)]
        public string ShipCity { get; set; } = string.Empty;
        [MaxLength(100)]
        public string ShipState { get; set; } = string.Empty;
        [MaxLength(10)]
        public string ShipPincode { get; set; } = string.Empty;

        // Coupon snapshot
        [MaxLength(30)]
        public string? CouponCode { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal CouponDiscount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Discount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DeliveryCharge { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        // Payment
        [MaxLength(20)]
        public string PaymentMethod { get; set; } = "razorpay"; // razorpay | cod

        [MaxLength(100)]
        public string? RazorpayOrderId { get; set; }
        [MaxLength(100)]
        public string? RazorpayPaymentId { get; set; }
        [MaxLength(200)]
        public string? RazorpaySignature { get; set; }

        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "pending"; // pending, paid, failed, refunded

        // Order status
        [Required, MaxLength(30)]
        public string Status { get; set; } = "placed"; // placed, confirmed, packed, shipped, out_for_delivery, delivered, cancelled

        public DateTime? EstimatedDelivery { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<TrackingEvent> TrackingEvents { get; set; } = new List<TrackingEvent>();
    }

    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        // Snapshot at purchase time
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Image { get; set; }
        [MaxLength(50)]
        public string? Sku { get; set; }
        [MaxLength(30)]
        public string? Color { get; set; }
        [MaxLength(10)]
        public string? Size { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PriceAtPurchase { get; set; }

        public int Quantity { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;
    }

    public class TrackingEvent
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }

        [Required, MaxLength(30)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;
    }

    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
        [ForeignKey("VariantId")]
        public ProductVariant Variant { get; set; } = null!;
    }

    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Type { get; set; } = "percentage"; // percentage | flat

        [Column(TypeName = "decimal(10,2)")]
        public decimal Value { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MinOrderAmount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal MaxDiscount { get; set; } = 0;

        public int UsageLimit { get; set; } = 100;
        public int UsedCount { get; set; } = 0;
        public int PerUserLimit { get; set; } = 1;

        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required, MaxLength(20)]
        public string Type { get; set; } = "system"; // order, promo, system

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        [MaxLength(500)]
        public string? Data { get; set; } // JSON string with extra data

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }

    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required, MaxLength(10)]
        public string Sender { get; set; } = "user"; // user | bot

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
