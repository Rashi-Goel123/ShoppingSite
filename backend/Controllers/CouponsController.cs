using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;

namespace EcommerceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CouponsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CouponsController(AppDbContext db) => _db = db;

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] ValidateCouponRequest request)
        {
            var coupon = await _db.Coupons.FirstOrDefaultAsync(c =>
                c.Code == request.Code.ToUpper() && c.IsActive &&
                c.ValidFrom <= DateTime.UtcNow && c.ValidUntil >= DateTime.UtcNow);

            if (coupon == null)
                return Ok(new CouponValidationResult(false, "Invalid or expired coupon", 0, request.Code));

            if (coupon.UsedCount >= coupon.UsageLimit)
                return Ok(new CouponValidationResult(false, "Coupon usage limit reached", 0, request.Code));

            if (request.OrderAmount < coupon.MinOrderAmount)
                return Ok(new CouponValidationResult(false, $"Minimum order amount is ₹{coupon.MinOrderAmount}", 0, request.Code));

            var discount = coupon.Type == "percentage"
                ? Math.Min(request.OrderAmount * coupon.Value / 100, coupon.MaxDiscount > 0 ? coupon.MaxDiscount : decimal.MaxValue)
                : coupon.Value;

            return Ok(new CouponValidationResult(true, $"Coupon applied! You save ₹{discount:N0}", discount, coupon.Code));
        }

        [HttpGet("available")]
        public async Task<IActionResult> Available()
        {
            var coupons = await _db.Coupons
                .Where(c => c.IsActive && c.ValidFrom <= DateTime.UtcNow && c.ValidUntil >= DateTime.UtcNow && c.UsedCount < c.UsageLimit)
                .Select(c => new CouponDto(c.Id, c.Code, c.Type, c.Value, c.MinOrderAmount, c.MaxDiscount, c.UsageLimit, c.UsedCount, c.ValidFrom, c.ValidUntil, c.IsActive))
                .ToListAsync();

            return Ok(coupons);
        }
    }
}
