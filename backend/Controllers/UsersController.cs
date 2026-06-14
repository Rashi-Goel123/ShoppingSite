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
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public UsersController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private int UserId => int.Parse(User.FindFirst("userId")!.Value);

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var user = await _db.Users.FindAsync(UserId);
            if (user == null) return NotFound();

            user.Name = request.Name;
            if (request.Email != null) user.Email = request.Email;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new UserDto(user.Id, user.Phone, user.Name, user.Email, user.Avatar, user.Role));
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { message = "No file uploaded" });
            if (file.Length > 5 * 1024 * 1024) return BadRequest(new { message = "File too large (max 5MB)" });

            var user = await _db.Users.FindAsync(UserId);
            if (user == null) return NotFound();

            var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads", "avatars");
            Directory.CreateDirectory(uploadsDir);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"avatar_{UserId}_{DateTime.UtcNow.Ticks}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            user.Avatar = $"/uploads/avatars/{fileName}";
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { url = user.Avatar });
        }

        [HttpPut("avatar/preset")]
        public async Task<IActionResult> SetPresetAvatar([FromBody] string avatarUrl)
        {
            var user = await _db.Users.FindAsync(UserId);
            if (user == null) return NotFound();

            user.Avatar = avatarUrl;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { url = user.Avatar });
        }

        // === ADDRESSES ===
        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses()
        {
            var addresses = await _db.Addresses
                .Where(a => a.UserId == UserId)
                .Select(a => new AddressDto(a.Id, a.Label, a.Street, a.City, a.State, a.Pincode, a.IsDefault))
                .ToListAsync();

            return Ok(addresses);
        }

        [HttpPost("addresses")]
        public async Task<IActionResult> AddAddress([FromBody] CreateAddressRequest request)
        {
            if (request.IsDefault)
            {
                var existing = await _db.Addresses.Where(a => a.UserId == UserId && a.IsDefault).ToListAsync();
                foreach (var a in existing) a.IsDefault = false;
            }

            var address = new Address
            {
                UserId = UserId,
                Label = request.Label,
                Street = request.Street,
                City = request.City,
                State = request.State,
                Pincode = request.Pincode,
                IsDefault = request.IsDefault
            };

            _db.Addresses.Add(address);
            await _db.SaveChangesAsync();

            return Created("", new AddressDto(address.Id, address.Label, address.Street, address.City, address.State, address.Pincode, address.IsDefault));
        }

        [HttpPut("addresses/{id}")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] CreateAddressRequest request)
        {
            var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId);
            if (address == null) return NotFound();

            if (request.IsDefault)
            {
                var others = await _db.Addresses.Where(a => a.UserId == UserId && a.Id != id && a.IsDefault).ToListAsync();
                foreach (var a in others) a.IsDefault = false;
            }

            address.Label = request.Label;
            address.Street = request.Street;
            address.City = request.City;
            address.State = request.State;
            address.Pincode = request.Pincode;
            address.IsDefault = request.IsDefault;
            await _db.SaveChangesAsync();

            return Ok(new AddressDto(address.Id, address.Label, address.Street, address.City, address.State, address.Pincode, address.IsDefault));
        }

        [HttpDelete("addresses/{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId);
            if (address == null) return NotFound();

            _db.Addresses.Remove(address);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Address deleted" });
        }

        // === WISHLIST ===
        [HttpGet("wishlist")]
        public async Task<IActionResult> GetWishlist()
        {
            var items = await _db.WishlistItems
                .Where(w => w.UserId == UserId)
                .Include(w => w.Product).ThenInclude(p => p.Images)
                .Include(w => w.Product).ThenInclude(p => p.Variants)
                .Include(w => w.Product).ThenInclude(p => p.Category)
                .Select(w => new ProductListDto(
                    w.Product.Id, w.Product.Title, w.Product.Slug, w.Product.Brand,
                    w.Product.Gender, w.Product.BasePrice,
                    w.Product.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault(),
                    w.Product.RatingAverage, w.Product.RatingCount, w.Product.IsFeatured,
                    w.Product.Category.Name,
                    w.Product.Variants.OrderBy(v => v.Price).Select(v => (decimal?)v.Price).FirstOrDefault(),
                    w.Product.Variants.OrderBy(v => v.Price).Select(v => (decimal?)v.Mrp).FirstOrDefault(),
                    null
                ))
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost("wishlist/{productId}")]
        public async Task<IActionResult> ToggleWishlist(int productId)
        {
            var existing = await _db.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == UserId && w.ProductId == productId);

            if (existing != null)
            {
                _db.WishlistItems.Remove(existing);
                await _db.SaveChangesAsync();
                return Ok(new { added = false, message = "Removed from wishlist" });
            }

            _db.WishlistItems.Add(new WishlistItem { UserId = UserId, ProductId = productId });
            await _db.SaveChangesAsync();
            return Ok(new { added = true, message = "Added to wishlist" });
        }

        [HttpGet("wishlist/ids")]
        public async Task<IActionResult> GetWishlistIds()
        {
            var ids = await _db.WishlistItems
                .Where(w => w.UserId == UserId)
                .Select(w => w.ProductId)
                .ToListAsync();

            return Ok(ids);
        }

        // === NOTIFICATIONS ===
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var notifications = await _db.Notifications
                .Where(n => n.UserId == UserId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .Select(n => new NotificationDto(n.Id, n.Type, n.Title, n.Message, n.IsRead, n.Data, n.CreatedAt))
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPut("notifications/{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == UserId);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Marked as read" });
        }

        [HttpPut("notifications/read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var unread = await _db.Notifications.Where(n => n.UserId == UserId && !n.IsRead).ToListAsync();
            foreach (var n in unread) n.IsRead = true;
            await _db.SaveChangesAsync();
            return Ok(new { message = "All marked as read" });
        }
    }
}
