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
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IRazorpayService _razorpay;
        private readonly IEmailService _email;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public OrdersController(AppDbContext db, IRazorpayService razorpay, IEmailService email, IHubContext<NotificationHub> notificationHub)
        {
            _db = db;
            _razorpay = razorpay;
            _email = email;
            _notificationHub = notificationHub;
        }

        private int UserId => int.Parse(User.FindFirst("userId")!.Value);

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            var user = await _db.Users.FindAsync(UserId);
            if (user == null) return Unauthorized();

            var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == UserId);
            if (address == null) return BadRequest(new { message = "Invalid address" });

            var cartItems = await _db.CartItems
                .Include(c => c.Product).ThenInclude(p => p.Images)
                .Include(c => c.Variant)
                .Where(c => c.UserId == UserId)
                .ToListAsync();

            if (!cartItems.Any()) return BadRequest(new { message = "Cart is empty" });

            // Verify Razorpay payment if applicable
            if (request.PaymentMethod == "razorpay")
            {
                if (string.IsNullOrEmpty(request.RazorpayPaymentId) || string.IsNullOrEmpty(request.RazorpaySignature))
                    return BadRequest(new { message = "Payment verification required" });

                var isValid = _razorpay.VerifySignature(request.RazorpayOrderId!, request.RazorpayPaymentId, request.RazorpaySignature);
                if (!isValid) return BadRequest(new { message = "Payment verification failed" });
            }

            // Calculate totals
            var subtotal = cartItems.Sum(c => c.Variant.Price * c.Quantity);
            decimal discount = 0;
            string? couponCode = null;

            if (!string.IsNullOrEmpty(request.CouponCode))
            {
                var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == request.CouponCode && c.IsActive && c.ValidFrom <= DateTime.UtcNow && c.ValidUntil >= DateTime.UtcNow);
                if (coupon != null && subtotal >= coupon.MinOrderAmount)
                {
                    discount = coupon.Type == "percentage"
                        ? Math.Min(subtotal * coupon.Value / 100, coupon.MaxDiscount > 0 ? coupon.MaxDiscount : decimal.MaxValue)
                        : coupon.Value;
                    couponCode = coupon.Code;
                    coupon.UsedCount++;
                }
            }

            var deliveryCharge = subtotal >= 499 ? 0 : 49;
            var totalAmount = subtotal - discount + deliveryCharge;

            // Generate order number
            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

            var order = new Order
            {
                OrderNumber = orderNumber,
                UserId = UserId,
                ShipStreet = address.Street,
                ShipCity = address.City,
                ShipState = address.State,
                ShipPincode = address.Pincode,
                CouponCode = couponCode,
                CouponDiscount = discount,
                Subtotal = subtotal,
                Discount = discount,
                DeliveryCharge = deliveryCharge,
                TotalAmount = totalAmount,
                PaymentMethod = request.PaymentMethod,
                RazorpayOrderId = request.RazorpayOrderId,
                RazorpayPaymentId = request.RazorpayPaymentId,
                RazorpaySignature = request.RazorpaySignature,
                PaymentStatus = request.PaymentMethod == "razorpay" ? "paid" : "pending",
                Status = "placed",
                EstimatedDelivery = DateTime.UtcNow.AddDays(5),
                Items = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Title = c.Product.Title,
                    Image = c.Product.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault(),
                    Sku = c.Variant.Sku,
                    Color = c.Variant.Color,
                    Size = c.Variant.Size,
                    PriceAtPurchase = c.Variant.Price,
                    Quantity = c.Quantity
                }).ToList(),
                TrackingEvents = new List<TrackingEvent>
                {
                    new() { Status = "placed", Description = "Order placed successfully", Timestamp = DateTime.UtcNow }
                }
            };

            _db.Orders.Add(order);

            // Reduce stock
            foreach (var item in cartItems)
            {
                item.Variant.Stock -= item.Quantity;
            }

            // Clear cart
            _db.CartItems.RemoveRange(cartItems);
            await _db.SaveChangesAsync();

            // Send email notification
            if (!string.IsNullOrEmpty(user.Email))
            {
                var itemsSummary = string.Join("", order.Items.Select(i =>
                    $"<p>{i.Title} ({i.Color} / {i.Size}) × {i.Quantity} = ₹{i.PriceAtPurchase * i.Quantity:N2}</p>"));
                _ = _email.SendOrderConfirmationAsync(user.Email, user.Name, orderNumber, totalAmount, itemsSummary);
            }

            return Created("", new { orderId = order.Id, orderNumber = order.OrderNumber, message = "Order placed successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _db.Orders
                .Where(o => o.UserId == UserId)
                .OrderByDescending(o => o.CreatedAt)
                .Include(o => o.Items)
                .Select(o => new OrderListDto(
                    o.Id, o.OrderNumber, o.Status, o.PaymentStatus,
                    o.TotalAmount, o.Items.Count,
                    o.Items.Select(i => i.Image).FirstOrDefault(),
                    o.CreatedAt
                ))
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.TrackingEvents.OrderBy(t => t.Timestamp))
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == UserId);

            if (order == null) return NotFound();

            var dto = new OrderDetailDto(
                order.Id, order.OrderNumber, order.Status, order.PaymentStatus,
                order.PaymentMethod, order.Subtotal, order.Discount, order.DeliveryCharge,
                order.TotalAmount, order.CouponCode,
                new AddressDto(0, "", order.ShipStreet, order.ShipCity, order.ShipState, order.ShipPincode, false),
                order.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.Title, i.Image, i.Sku, i.Color, i.Size, i.PriceAtPurchase, i.Quantity)).ToList(),
                order.TrackingEvents.Select(t => new TrackingEventDto(t.Status, t.Description, t.Location, t.Timestamp)).ToList(),
                order.EstimatedDelivery, order.CreatedAt
            );

            return Ok(dto);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .Include(o => o.TrackingEvents)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == UserId);

            if (order == null) return NotFound();
            if (order.Status == "delivered" || order.Status == "cancelled")
                return BadRequest(new { message = "Cannot cancel this order" });

            order.Status = "cancelled";
            order.PaymentStatus = order.PaymentStatus == "paid" ? "refunded" : "cancelled";
            order.TrackingEvents.Add(new TrackingEvent
            {
                Status = "cancelled",
                Description = "Order cancelled by customer",
                Timestamp = DateTime.UtcNow
            });

            // Restore stock
            foreach (var item in order.Items)
            {
                var variant = await _db.ProductVariants.FirstOrDefaultAsync(v => v.Sku == item.Sku);
                if (variant != null) variant.Stock += item.Quantity;
            }

            await _db.SaveChangesAsync();

            // Email
            if (!string.IsNullOrEmpty(order.User.Email))
                _ = _email.SendShippingUpdateAsync(order.User.Email, order.User.Name, order.OrderNumber, "cancelled", "Your order has been cancelled.");

            return Ok(new { message = "Order cancelled successfully" });
        }
    }
}
