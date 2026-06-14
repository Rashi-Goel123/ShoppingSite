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
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CartController(AppDbContext db) => _db = db;

        private int UserId => int.Parse(User.FindFirst("userId")!.Value);

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var items = await _db.CartItems
                .Where(c => c.UserId == UserId)
                .Include(c => c.Product).ThenInclude(p => p.Images)
                .Include(c => c.Variant)
                .Select(c => new CartItemDto(
                    c.Id, c.ProductId, c.VariantId,
                    c.Product.Title,
                    c.Product.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault(),
                    c.Variant.Color, c.Variant.Size,
                    c.Variant.Price, c.Variant.Mrp,
                    c.Quantity, c.Variant.Stock
                ))
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] AddToCartRequest request)
        {
            var variant = await _db.ProductVariants.FindAsync(request.VariantId);
            if (variant == null) return NotFound(new { message = "Variant not found" });
            if (variant.Stock < request.Quantity) return BadRequest(new { message = "Not enough stock" });

            var existing = await _db.CartItems
                .FirstOrDefaultAsync(c => c.UserId == UserId && c.VariantId == request.VariantId);

            if (existing != null)
            {
                existing.Quantity += request.Quantity;
                if (existing.Quantity > variant.Stock)
                    existing.Quantity = variant.Stock;
            }
            else
            {
                _db.CartItems.Add(new CartItem
                {
                    UserId = UserId,
                    ProductId = request.ProductId,
                    VariantId = request.VariantId,
                    Quantity = request.Quantity
                });
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Added to cart" });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateCartRequest request)
        {
            var item = await _db.CartItems
                .Include(c => c.Variant)
                .FirstOrDefaultAsync(c => c.Id == request.CartItemId && c.UserId == UserId);

            if (item == null) return NotFound();

            if (request.Quantity <= 0)
            {
                _db.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = Math.Min(request.Quantity, item.Variant.Stock);
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Cart updated" });
        }

        [HttpDelete("remove/{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            var item = await _db.CartItems.FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId);
            if (item == null) return NotFound();

            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Removed from cart" });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> Clear()
        {
            var items = await _db.CartItems.Where(c => c.UserId == UserId).ToListAsync();
            _db.CartItems.RemoveRange(items);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Cart cleared" });
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            var count = await _db.CartItems.Where(c => c.UserId == UserId).SumAsync(c => c.Quantity);
            return Ok(new { count });
        }
    }
}
