using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Data;
using EcommerceApi.DTOs;

namespace EcommerceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ProductsController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductFilterParams filter)
        {
            var query = _db.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            // Filters
            if (!string.IsNullOrEmpty(filter.Category))
                query = query.Where(p => p.Category.Slug == filter.Category || (p.Category.Parent != null && p.Category.Parent.Slug == filter.Category));

            if (!string.IsNullOrEmpty(filter.Gender))
                query = query.Where(p => p.Gender == filter.Gender);

            if (!string.IsNullOrEmpty(filter.Brand))
                query = query.Where(p => p.Brand == filter.Brand);

            if (!string.IsNullOrEmpty(filter.Color))
                query = query.Where(p => p.Variants.Any(v => v.Color == filter.Color));

            if (!string.IsNullOrEmpty(filter.Size))
                query = query.Where(p => p.Variants.Any(v => v.Size == filter.Size));

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.BasePrice >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.BasePrice <= filter.MaxPrice.Value);

            if (!string.IsNullOrEmpty(filter.Search))
                query = query.Where(p => p.Title.Contains(filter.Search) || p.Brand.Contains(filter.Search) || p.Description.Contains(filter.Search));

            // Sort
            query = filter.SortBy switch
            {
                "price_asc" => query.OrderBy(p => p.BasePrice),
                "price_desc" => query.OrderByDescending(p => p.BasePrice),
                "rating" => query.OrderByDescending(p => p.RatingAverage),
                "popular" => query.OrderByDescending(p => p.RatingCount),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new ProductListDto(
                    p.Id, p.Title, p.Slug, p.Brand, p.Gender, p.BasePrice,
                    p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault(),
                    p.RatingAverage, p.RatingCount, p.IsFeatured, p.Category.Name,
                    p.Variants.OrderBy(v => v.Price).Select(v => (decimal?)v.Price).FirstOrDefault(),
                    p.Variants.OrderBy(v => v.Price).Select(v => (decimal?)v.Mrp).FirstOrDefault(),
                    p.Variants.Any() ? (int?)((1 - p.Variants.OrderBy(v => v.Price).First().Price / p.Variants.OrderBy(v => v.Price).First().Mrp) * 100) : null
                ))
                .ToListAsync();

            return Ok(new PagedResult<ProductListDto>(items, totalCount, filter.Page, filter.PageSize, (int)Math.Ceiling(totalCount / (double)filter.PageSize)));
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var product = await _db.Products
                .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
                .Include(p => p.Variants)
                .Include(p => p.Category).ThenInclude(c => c.Parent)
                .Include(p => p.Reviews.OrderByDescending(r => r.CreatedAt).Take(10))
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

            if (product == null) return NotFound();

            var dto = new ProductDetailDto(
                product.Id, product.Title, product.Slug, product.Description,
                product.Brand, product.Gender, product.Material, product.BasePrice,
                product.RatingAverage, product.RatingCount, product.IsFeatured,
                new CategoryDto(product.Category.Id, product.Category.Name, product.Category.Slug, product.Category.Icon, product.Category.ParentId, null),
                product.Images.Select(i => new ProductImageDto(i.Id, i.Url, i.DisplayOrder)).ToList(),
                product.Variants.Select(v => new ProductVariantDto(v.Id, v.Sku, v.Color, v.ColorHex, v.Size, v.Price, v.Mrp, v.Stock)).ToList(),
                product.Reviews.Select(r => new ReviewDto(r.Id, r.Rating, r.Title, r.Comment, r.User.Name, r.IsVerifiedPurchase, r.CreatedAt)).ToList()
            );

            return Ok(dto);
        }

        [HttpGet("featured")]
        public async Task<IActionResult> GetFeatured()
        {
            var products = await _db.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.IsFeatured)
                .Take(8)
                .Select(p => new ProductListDto(
                    p.Id, p.Title, p.Slug, p.Brand, p.Gender, p.BasePrice,
                    p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault(),
                    p.RatingAverage, p.RatingCount, p.IsFeatured, p.Category.Name,
                    p.Variants.OrderBy(v => v.Price).Select(v => (decimal?)v.Price).FirstOrDefault(),
                    p.Variants.OrderBy(v => v.Price).Select(v => (decimal?)v.Mrp).FirstOrDefault(),
                    null
                ))
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("new-arrivals")]
        public async Task<IActionResult> GetNewArrivals()
        {
            var products = await _db.Products
                .Include(p => p.Images).Include(p => p.Variants).Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .Select(p => new ProductListDto(
                    p.Id, p.Title, p.Slug, p.Brand, p.Gender, p.BasePrice,
                    p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault(),
                    p.RatingAverage, p.RatingCount, p.IsFeatured, p.Category.Name,
                    p.Variants.OrderBy(v => v.Price).Select(v => (decimal?)v.Price).FirstOrDefault(),
                    p.Variants.OrderBy(v => v.Price).Select(v => (decimal?)v.Mrp).FirstOrDefault(),
                    null
                ))
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _db.Categories
                .Where(c => c.IsActive && c.ParentId == null)
                .Include(c => c.Children.Where(ch => ch.IsActive))
                .Select(c => new CategoryDto(
                    c.Id, c.Name, c.Slug, c.Icon, null,
                    c.Children.Select(ch => new CategoryDto(ch.Id, ch.Name, ch.Slug, ch.Icon, ch.ParentId, null)).ToList()
                ))
                .ToListAsync();

            return Ok(categories);
        }

        [HttpGet("brands")]
        public async Task<IActionResult> GetBrands()
        {
            var brands = await _db.Products
                .Where(p => p.IsActive)
                .Select(p => p.Brand)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();

            return Ok(brands);
        }

        [Authorize]
        [HttpPost("{id}/review")]
        public async Task<IActionResult> AddReview(int id, [FromBody] CreateReviewRequest request)
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Check for existing review
            if (await _db.Reviews.AnyAsync(r => r.UserId == userId && r.ProductId == id))
                return Conflict(new { message = "You have already reviewed this product" });

            var isVerified = await _db.Orders
                .AnyAsync(o => o.UserId == userId && o.Items.Any(i => i.ProductId == id) && o.Status == "delivered");

            var review = new Models.Review
            {
                ProductId = id,
                UserId = userId,
                Rating = Math.Clamp(request.Rating, 1, 5),
                Title = request.Title,
                Comment = request.Comment,
                IsVerifiedPurchase = isVerified
            };
            _db.Reviews.Add(review);

            // Update product rating
            var allReviews = await _db.Reviews.Where(r => r.ProductId == id).ToListAsync();
            allReviews.Add(review);
            product.RatingAverage = (decimal)allReviews.Average(r => r.Rating);
            product.RatingCount = allReviews.Count;

            await _db.SaveChangesAsync();
            return Created("", new { message = "Review added successfully" });
        }
    }
}
