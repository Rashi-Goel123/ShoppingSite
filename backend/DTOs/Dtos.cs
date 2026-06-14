namespace EcommerceApi.DTOs
{
    // ===== AUTH =====
    public record SendOtpRequest(string Phone);
    public record VerifyOtpRequest(string Phone, string Code);
    public record AdminRegisterRequest(string Name, string Email, string Phone, string Password, string InviteCode);
    public record AdminLoginRequest(string Email, string Password);
    public record AuthResponse(string Token, UserDto User);

    // ===== USER =====
    public record UserDto(int Id, string Phone, string Name, string? Email, string? Avatar, string Role);
    public record UpdateProfileRequest(string Name, string? Email);
    public record AddressDto(int Id, string Label, string Street, string City, string State, string Pincode, bool IsDefault);
    public record CreateAddressRequest(string Label, string Street, string City, string State, string Pincode, bool IsDefault);

    // ===== PRODUCT =====
    public record CategoryDto(int Id, string Name, string Slug, string? Icon, int? ParentId, List<CategoryDto>? Children);
    public record ProductListDto(int Id, string Title, string Slug, string Brand, string Gender, decimal BasePrice,
        string? FirstImage, decimal RatingAverage, int RatingCount, bool IsFeatured, string CategoryName,
        decimal? DiscountedPrice, decimal? Mrp, int? DiscountPercent);
    public record ProductDetailDto(int Id, string Title, string Slug, string Description, string Brand,
        string Gender, string? Material, decimal BasePrice, decimal RatingAverage, int RatingCount,
        bool IsFeatured, CategoryDto Category, List<ProductImageDto> Images,
        List<ProductVariantDto> Variants, List<ReviewDto>? Reviews);
    public record ProductImageDto(int Id, string Url, int DisplayOrder);
    public record ProductVariantDto(int Id, string Sku, string? Color, string? ColorHex, string? Size,
        decimal Price, decimal Mrp, int Stock);
    public record ReviewDto(int Id, int Rating, string? Title, string? Comment, string UserName,
        bool IsVerifiedPurchase, DateTime CreatedAt);
    public record CreateReviewRequest(int Rating, string? Title, string? Comment);

    // ===== PRODUCT FILTER =====
    public class ProductFilterParams
    {
        public string? Category { get; set; }
        public string? Gender { get; set; }
        public string? Brand { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "newest"; // newest, price_asc, price_desc, rating, popular
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
    public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);

    // ===== CART =====
    public record CartItemDto(int Id, int ProductId, int VariantId, string Title, string? Image,
        string? Color, string? Size, decimal Price, decimal Mrp, int Quantity, int Stock);
    public record AddToCartRequest(int ProductId, int VariantId, int Quantity = 1);
    public record UpdateCartRequest(int CartItemId, int Quantity);

    // ===== ORDER =====
    public record PlaceOrderRequest(int AddressId, string PaymentMethod, string? CouponCode,
        string? RazorpayOrderId, string? RazorpayPaymentId, string? RazorpaySignature);
    public record OrderListDto(int Id, string OrderNumber, string Status, string PaymentStatus,
        decimal TotalAmount, int ItemCount, string? FirstItemImage, DateTime CreatedAt);
    public record OrderDetailDto(int Id, string OrderNumber, string Status, string PaymentStatus,
        string PaymentMethod, decimal Subtotal, decimal Discount, decimal DeliveryCharge,
        decimal TotalAmount, string? CouponCode, AddressDto ShippingAddress,
        List<OrderItemDto> Items, List<TrackingEventDto> Tracking,
        DateTime? EstimatedDelivery, DateTime CreatedAt);
    public record OrderItemDto(int Id, int ProductId, string Title, string? Image, string? Sku,
        string? Color, string? Size, decimal PriceAtPurchase, int Quantity);
    public record TrackingEventDto(string Status, string? Description, string? Location, DateTime Timestamp);
    public record UpdateOrderStatusRequest(string Status, string? Description, string? Location);

    // ===== COUPON =====
    public record CouponDto(int Id, string Code, string Type, decimal Value, decimal MinOrderAmount,
        decimal MaxDiscount, int UsageLimit, int UsedCount, DateTime ValidFrom, DateTime ValidUntil, bool IsActive);
    public record ValidateCouponRequest(string Code, decimal OrderAmount);
    public record CouponValidationResult(bool IsValid, string? Message, decimal DiscountAmount, string Code);
    public record CreateCouponRequest(string Code, string Type, decimal Value, decimal MinOrderAmount,
        decimal MaxDiscount, int UsageLimit, int PerUserLimit, DateTime ValidFrom, DateTime ValidUntil);

    // ===== PAYMENT =====
    public record CreatePaymentOrderRequest(decimal Amount);
    public record PaymentOrderResponse(string OrderId, decimal Amount, string Currency, string KeyId);
    public record VerifyPaymentRequest(string RazorpayOrderId, string RazorpayPaymentId, string RazorpaySignature);

    // ===== NOTIFICATION =====
    public record NotificationDto(int Id, string Type, string Title, string Message, bool IsRead, string? Data, DateTime CreatedAt);

    // ===== ADMIN DASHBOARD =====
    public record DashboardStats(decimal TotalRevenue, int TotalOrders, int TotalUsers, int TotalProducts,
        int PendingOrders, int DeliveredOrders, List<RecentOrderDto> RecentOrders, List<TopProductDto> TopProducts);
    public record RecentOrderDto(int Id, string OrderNumber, string UserName, decimal TotalAmount, string Status, DateTime CreatedAt);
    public record TopProductDto(int ProductId, string Title, string? Image, int TotalSold, decimal TotalRevenue);

    // ===== ADMIN PRODUCT =====
    public record CreateProductRequest(string Title, string Description, int CategoryId, string Brand,
        string Gender, string? Material, decimal BasePrice, bool IsFeatured, List<CreateVariantRequest> Variants);
    public record CreateVariantRequest(string Sku, string? Color, string? ColorHex, string? Size,
        decimal Price, decimal Mrp, int Stock);
}
