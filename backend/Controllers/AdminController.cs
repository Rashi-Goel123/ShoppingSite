using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using EcommerceApi.Services;
using EcommerceApi.Hubs;

namespace EcommerceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _email;
        private readonly IHubContext<OrderTrackingHub> _orderHub;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly IWebHostEnvironment _env;

        public AdminController(AppDbContext db, IEmailService email, IHubContext<OrderTrackingHub> orderHub, IHubContext<NotificationHub> notificationHub, IWebHostEnvironment env)
        {
            _db = db;
            _email = email;
            _orderHub = orderHub;
            _notificationHub = notificationHub;
            _env = env;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var totalRevenue = await _db.Orders.Where(o => o.PaymentStatus == "paid").SumAsync(o => o.TotalAmount);
            var totalOrders = await _db.Orders.CountAsync();
            var totalUsers = await _db.Users.CountAsync(u => u.Role == "user");
            var totalProducts = await _db.Products.CountAsync(p => !p.IsDeleted);
            var pendingOrders = await _db.Orders.CountAsync(o => o.Status != "delivered" && o.Status != "cancelled");
            var deliveredOrders = await _db.Orders.CountAsync(o => o.Status == "delivered");

            var recentOrders = await _db.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new RecentOrderDto(o.Id, o.OrderNumber, o.User.Name, o.TotalAmount, o.Status, o.CreatedAt))
                .ToListAsync();

            var topProducts = await _db.OrderItems
                .GroupBy(i => new { i.ProductId, i.Title, i.Image })
                .Select(g => new TopProductDto(g.Key.ProductId, g.Key.Title, g.Key.Image, g.Sum(i => i.Quantity), g.Sum(i => i.PriceAtPurchase * i.Quantity)))
                .OrderByDescending(p => p.TotalSold)
                .Take(5)
                .ToListAsync();

            return Ok(new DashboardStats(totalRevenue, totalOrders, totalUsers, totalProducts, pendingOrders, deliveredOrders, recentOrders, topProducts));
        }

        // === PRODUCT MANAGEMENT ===
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _db.Products.IgnoreQueryFilters().Include(p => p.Category).Include(p => p.Variants).Include(p => p.Images);
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(p => p.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new { items, total, page, pageSize });
        }

        [HttpPost("products")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            var slug = request.Title.ToLower().Replace(" ", "-").Replace("'", "");
            // Ensure unique slug
            if (await _db.Products.AnyAsync(p => p.Slug == slug))
                slug += $"-{new Random().Next(100, 999)}";

            var product = new Product
            {
                Title = request.Title,
                Slug = slug,
                Description = request.Description,
                CategoryId = request.CategoryId,
                Brand = request.Brand,
                Gender = request.Gender,
                Material = request.Material,
                BasePrice = request.BasePrice,
                IsFeatured = request.IsFeatured,
                Variants = request.Variants.Select(v => new ProductVariant
                {
                    Sku = v.Sku,
                    Color = v.Color,
                    ColorHex = v.ColorHex,
                    Size = v.Size,
                    Price = v.Price,
                    Mrp = v.Mrp,
                    Stock = v.Stock
                }).ToList()
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return Created("", new { id = product.Id, slug = product.Slug });
        }

        [HttpPut("products/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] CreateProductRequest request)
        {
            var product = await _db.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            product.Title = request.Title;
            product.Description = request.Description;
            product.CategoryId = request.CategoryId;
            product.Brand = request.Brand;
            product.Gender = request.Gender;
            product.Material = request.Material;
            product.BasePrice = request.BasePrice;
            product.IsFeatured = request.IsFeatured;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Product updated" });
        }

        [HttpDelete("products/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.IsDeleted = true;
            product.IsActive = false;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Product deleted" });
        }

        [HttpPost("products/{id}/images")]
        public async Task<IActionResult> UploadProductImage(int id, IFormFile file)
        {
            var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads", "products");
            Directory.CreateDirectory(uploadsDir);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"prod_{id}_{DateTime.UtcNow.Ticks}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var image = new ProductImage
            {
                ProductId = id,
                Url = $"/uploads/products/{fileName}",
                DisplayOrder = product.Images.Count
            };
            _db.ProductImages.Add(image);
            await _db.SaveChangesAsync();

            return Ok(new { id = image.Id, url = image.Url });
        }

        // === ORDER MANAGEMENT ===
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _db.Orders.Include(o => o.User).Include(o => o.Items).AsQueryable();
            if (!string.IsNullOrEmpty(status)) query = query.Where(o => o.Status == status);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(o => new
                {
                    o.Id, o.OrderNumber, o.Status, o.PaymentStatus, o.PaymentMethod,
                    o.TotalAmount, o.CreatedAt,
                    UserName = o.User.Name, UserPhone = o.User.Phone,
                    ItemCount = o.Items.Count
                })
                .ToListAsync();

            return Ok(new { items, total, page, pageSize });
        }

        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            var order = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.TrackingEvents)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            order.Status = request.Status;
            order.UpdatedAt = DateTime.UtcNow;
            order.TrackingEvents.Add(new TrackingEvent
            {
                Status = request.Status,
                Description = request.Description ?? $"Order {request.Status.Replace("_", " ")}",
                Location = request.Location,
                Timestamp = DateTime.UtcNow
            });

            if (request.Status == "delivered")
                order.PaymentStatus = "paid";

            await _db.SaveChangesAsync();

            // Send real-time update via SignalR
            await _orderHub.Clients.Group($"order_{id}").SendAsync("OrderStatusUpdated", new
            {
                orderId = id,
                status = request.Status,
                description = request.Description,
                timestamp = DateTime.UtcNow
            });

            // Send notification
            var notification = new Notification
            {
                UserId = order.UserId,
                Type = "order",
                Title = $"Order {order.OrderNumber} {request.Status.Replace("_", " ")}",
                Message = request.Description ?? $"Your order status has been updated to {request.Status.Replace("_", " ")}",
                Data = $"{{\"orderId\":{id}}}"
            };
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            await _notificationHub.Clients.Group($"user_{order.UserId}").SendAsync("NewNotification", new NotificationDto(
                notification.Id, notification.Type, notification.Title, notification.Message, false, notification.Data, notification.CreatedAt));

            // Send email
            if (!string.IsNullOrEmpty(order.User.Email))
                _ = _email.SendShippingUpdateAsync(order.User.Email, order.User.Name, order.OrderNumber, request.Status, request.Description);

            return Ok(new { message = "Order status updated" });
        }

        // === USER MANAGEMENT ===
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var total = await _db.Users.CountAsync();
            var users = await _db.Users
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(u => new { u.Id, u.Name, u.Phone, u.Email, u.Role, u.Avatar, u.CreatedAt, OrderCount = u.Orders.Count })
                .ToListAsync();

            return Ok(new { items = users, total, page, pageSize });
        }

        // === COUPON MANAGEMENT ===
        [HttpGet("coupons")]
        public async Task<IActionResult> GetCoupons()
        {
            var coupons = await _db.Coupons
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CouponDto(c.Id, c.Code, c.Type, c.Value, c.MinOrderAmount, c.MaxDiscount, c.UsageLimit, c.UsedCount, c.ValidFrom, c.ValidUntil, c.IsActive))
                .ToListAsync();

            return Ok(coupons);
        }

        [HttpPost("coupons")]
        public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponRequest request)
        {
            if (await _db.Coupons.AnyAsync(c => c.Code == request.Code.ToUpper()))
                return Conflict(new { message = "Coupon code already exists" });

            var coupon = new Coupon
            {
                Code = request.Code.ToUpper(),
                Type = request.Type,
                Value = request.Value,
                MinOrderAmount = request.MinOrderAmount,
                MaxDiscount = request.MaxDiscount,
                UsageLimit = request.UsageLimit,
                PerUserLimit = request.PerUserLimit,
                ValidFrom = request.ValidFrom,
                ValidUntil = request.ValidUntil
            };

            _db.Coupons.Add(coupon);
            await _db.SaveChangesAsync();

            return Created("", new { id = coupon.Id, code = coupon.Code });
        }

        [HttpDelete("coupons/{id}")]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            var coupon = await _db.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();

            _db.Coupons.Remove(coupon);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Coupon deleted" });
        }

        // === CATEGORY MANAGEMENT ===
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDto request)
        {
            var slug = request.Name.ToLower().Replace(" ", "-").Replace("'", "").Replace("&", "and");
            var category = new Category
            {
                Name = request.Name,
                Slug = slug,
                Icon = request.Icon,
                ParentId = request.ParentId
            };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return Created("", new { id = category.Id, slug = category.Slug });
        }
    }
}
