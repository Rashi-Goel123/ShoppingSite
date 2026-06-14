using Microsoft.EntityFrameworkCore;
using EcommerceApi.Data;
using EcommerceApi.Models;

namespace EcommerceApi.Seeds
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Categories.AnyAsync()) return; // Already seeded

            // === CATEGORIES ===
            var menswear = new Category { Name = "Men's Wear", Slug = "mens-wear", Icon = "👔" };
            var womenswear = new Category { Name = "Women's Wear", Slug = "womens-wear", Icon = "👗" };
            var footwear = new Category { Name = "Footwear", Slug = "footwear", Icon = "👟" };
            var accessories = new Category { Name = "Accessories", Slug = "accessories", Icon = "💎" };
            var kidswear = new Category { Name = "Kids' Wear", Slug = "kids-wear", Icon = "🧒" };
            var beauty = new Category { Name = "Beauty & Grooming", Slug = "beauty-and-grooming", Icon = "💄" };

            db.Categories.AddRange(menswear, womenswear, footwear, accessories, kidswear, beauty);
            await db.SaveChangesAsync();

            // Sub-categories
            var subcats = new List<Category>
            {
                new() { Name = "T-Shirts & Polos", Slug = "t-shirts-polos", ParentId = menswear.Id },
                new() { Name = "Shirts", Slug = "shirts", ParentId = menswear.Id },
                new() { Name = "Jeans & Trousers", Slug = "jeans-trousers", ParentId = menswear.Id },
                new() { Name = "Jackets & Blazers", Slug = "jackets-blazers", ParentId = menswear.Id },
                new() { Name = "Ethnic Wear", Slug = "ethnic-wear-men", ParentId = menswear.Id },
                new() { Name = "Activewear", Slug = "activewear-men", ParentId = menswear.Id },

                new() { Name = "Dresses", Slug = "dresses", ParentId = womenswear.Id },
                new() { Name = "Tops & Tees", Slug = "tops-tees", ParentId = womenswear.Id },
                new() { Name = "Sarees & Lehengas", Slug = "sarees-lehengas", ParentId = womenswear.Id },
                new() { Name = "Jeans & Palazzos", Slug = "jeans-palazzos", ParentId = womenswear.Id },
                new() { Name = "Jackets & Coats", Slug = "jackets-coats", ParentId = womenswear.Id },

                new() { Name = "Sneakers", Slug = "sneakers", ParentId = footwear.Id },
                new() { Name = "Formal Shoes", Slug = "formal-shoes", ParentId = footwear.Id },
                new() { Name = "Sandals & Flats", Slug = "sandals-flats", ParentId = footwear.Id },
                new() { Name = "Sports Shoes", Slug = "sports-shoes", ParentId = footwear.Id },
                new() { Name = "Boots", Slug = "boots", ParentId = footwear.Id },

                new() { Name = "Watches", Slug = "watches", ParentId = accessories.Id },
                new() { Name = "Bags & Wallets", Slug = "bags-wallets", ParentId = accessories.Id },
                new() { Name = "Sunglasses", Slug = "sunglasses", ParentId = accessories.Id },
                new() { Name = "Jewellery", Slug = "jewellery", ParentId = accessories.Id },
                new() { Name = "Belts", Slug = "belts", ParentId = accessories.Id },

                new() { Name = "Boys' Clothing", Slug = "boys-clothing", ParentId = kidswear.Id },
                new() { Name = "Girls' Clothing", Slug = "girls-clothing", ParentId = kidswear.Id },

                new() { Name = "Skincare", Slug = "skincare", ParentId = beauty.Id },
                new() { Name = "Fragrances", Slug = "fragrances", ParentId = beauty.Id },
                new() { Name = "Haircare", Slug = "haircare", ParentId = beauty.Id },
            };
            db.Categories.AddRange(subcats);
            await db.SaveChangesAsync();

            // Get subcategory IDs
            var tshirts = subcats[0].Id;
            var shirts = subcats[1].Id;
            var jeans = subcats[2].Id;
            var dresses = subcats[6].Id;
            var tops = subcats[7].Id;
            var sneakersId = subcats[11].Id;
            var watchesId = subcats[16].Id;
            var bags = subcats[17].Id;

            // === PRODUCTS ===
            var products = new List<Product>
            {
                new()
                {
                    Title = "Classic Cotton Crew Neck T-Shirt", Slug = "classic-cotton-crew-neck-tshirt",
                    Description = "Premium 100% cotton crew neck t-shirt with a relaxed fit. Perfect for everyday casual wear. Pre-shrunk fabric with reinforced stitching for durability.",
                    CategoryId = tshirts, Brand = "Urban Edge", Gender = "men", Material = "Cotton",
                    BasePrice = 599, IsFeatured = true,
                    Images = new List<ProductImage>
                    {
                        new() { Url = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=600", DisplayOrder = 0 },
                        new() { Url = "https://images.unsplash.com/photo-1622445275576-721325763afe?w=600", DisplayOrder = 1 }
                    },
                    Variants = new List<ProductVariant>
                    {
                        new() { Sku = "UE-TCN-BLK-S", Color = "Black", ColorHex = "#000000", Size = "S", Price = 499, Mrp = 799, Stock = 50 },
                        new() { Sku = "UE-TCN-BLK-M", Color = "Black", ColorHex = "#000000", Size = "M", Price = 499, Mrp = 799, Stock = 75 },
                        new() { Sku = "UE-TCN-BLK-L", Color = "Black", ColorHex = "#000000", Size = "L", Price = 499, Mrp = 799, Stock = 60 },
                        new() { Sku = "UE-TCN-WHT-M", Color = "White", ColorHex = "#FFFFFF", Size = "M", Price = 499, Mrp = 799, Stock = 45 },
                        new() { Sku = "UE-TCN-NVY-L", Color = "Navy", ColorHex = "#1B2A4A", Size = "L", Price = 549, Mrp = 849, Stock = 30 },
                    }
                },
                new()
                {
                    Title = "Slim Fit Oxford Button-Down Shirt", Slug = "slim-fit-oxford-button-down",
                    Description = "A timeless Oxford button-down in premium brushed cotton. Features a slim silhouette, button-down collar, and a curved hem.",
                    CategoryId = shirts, Brand = "Formal Craft", Gender = "men", Material = "Cotton Blend",
                    BasePrice = 1299, IsFeatured = true,
                    Images = new List<ProductImage>
                    {
                        new() { Url = "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=600", DisplayOrder = 0 }
                    },
                    Variants = new List<ProductVariant>
                    {
                        new() { Sku = "FC-OXF-LBL-M", Color = "Light Blue", ColorHex = "#ADD8E6", Size = "M", Price = 1099, Mrp = 1599, Stock = 40 },
                        new() { Sku = "FC-OXF-LBL-L", Color = "Light Blue", ColorHex = "#ADD8E6", Size = "L", Price = 1099, Mrp = 1599, Stock = 35 },
                        new() { Sku = "FC-OXF-WHT-M", Color = "White", ColorHex = "#FFFFFF", Size = "M", Price = 1099, Mrp = 1599, Stock = 50 },
                        new() { Sku = "FC-OXF-PNK-L", Color = "Pink", ColorHex = "#FFB6C1", Size = "L", Price = 1099, Mrp = 1599, Stock = 25 },
                    }
                },
                new()
                {
                    Title = "Stretch Skinny Fit Jeans", Slug = "stretch-skinny-fit-jeans",
                    Description = "Modern skinny jeans with 2% elastane for comfortable stretch. Dark indigo wash with whiskering details. Five-pocket styling.",
                    CategoryId = jeans, Brand = "Denim Republic", Gender = "men", Material = "Denim (98% Cotton, 2% Elastane)",
                    BasePrice = 1799,
                    Images = new List<ProductImage>
                    {
                        new() { Url = "https://images.unsplash.com/photo-1542272604-787c3835535d?w=600", DisplayOrder = 0 }
                    },
                    Variants = new List<ProductVariant>
                    {
                        new() { Sku = "DR-SKN-DRK-30", Color = "Dark Indigo", ColorHex = "#2B2D42", Size = "30", Price = 1499, Mrp = 2299, Stock = 30 },
                        new() { Sku = "DR-SKN-DRK-32", Color = "Dark Indigo", ColorHex = "#2B2D42", Size = "32", Price = 1499, Mrp = 2299, Stock = 45 },
                        new() { Sku = "DR-SKN-BLU-32", Color = "Medium Blue", ColorHex = "#4A90D9", Size = "32", Price = 1499, Mrp = 2299, Stock = 35 },
                        new() { Sku = "DR-SKN-BLK-34", Color = "Black", ColorHex = "#000000", Size = "34", Price = 1499, Mrp = 2299, Stock = 20 },
                    }
                },
                new()
                {
                    Title = "Floral Print Maxi Dress", Slug = "floral-print-maxi-dress",
                    Description = "Elegant floor-length dress with vibrant floral print. V-neckline with adjustable spaghetti straps. Flowing A-line silhouette.",
                    CategoryId = dresses, Brand = "Blossom Lane", Gender = "women", Material = "Viscose",
                    BasePrice = 2499, IsFeatured = true,
                    Images = new List<ProductImage>
                    {
                        new() { Url = "https://images.unsplash.com/photo-1572804013309-59a88b7e92f1?w=600", DisplayOrder = 0 }
                    },
                    Variants = new List<ProductVariant>
                    {
                        new() { Sku = "BL-MXD-FLR-S", Color = "Floral Multi", ColorHex = "#FF6B9D", Size = "S", Price = 1999, Mrp = 3499, Stock = 20 },
                        new() { Sku = "BL-MXD-FLR-M", Color = "Floral Multi", ColorHex = "#FF6B9D", Size = "M", Price = 1999, Mrp = 3499, Stock = 30 },
                        new() { Sku = "BL-MXD-FLR-L", Color = "Floral Multi", ColorHex = "#FF6B9D", Size = "L", Price = 1999, Mrp = 3499, Stock = 25 },
                    }
                },
                new()
                {
                    Title = "Ribbed Crop Top", Slug = "ribbed-crop-top",
                    Description = "Soft ribbed knit crop top with a flattering fit. Square neckline and short puff sleeves. Pair with high-waisted jeans or skirts.",
                    CategoryId = tops, Brand = "Style Hub", Gender = "women", Material = "Cotton Rib",
                    BasePrice = 699,
                    Images = new List<ProductImage>
                    {
                        new() { Url = "https://images.unsplash.com/photo-1594938298603-c8148c4dae35?w=600", DisplayOrder = 0 }
                    },
                    Variants = new List<ProductVariant>
                    {
                        new() { Sku = "SH-RCT-BLK-S", Color = "Black", ColorHex = "#000000", Size = "S", Price = 549, Mrp = 899, Stock = 40 },
                        new() { Sku = "SH-RCT-WHT-M", Color = "White", ColorHex = "#FFFFFF", Size = "M", Price = 549, Mrp = 899, Stock = 50 },
                        new() { Sku = "SH-RCT-OLV-L", Color = "Olive", ColorHex = "#808000", Size = "L", Price = 549, Mrp = 899, Stock = 30 },
                    }
                },
                new()
                {
                    Title = "Air Mesh Running Sneakers", Slug = "air-mesh-running-sneakers",
                    Description = "Lightweight breathable mesh upper with responsive cushioning. Rubber outsole for excellent grip. Perfect for running and gym workouts.",
                    CategoryId = sneakersId, Brand = "StepX", Gender = "unisex", Material = "Mesh/Rubber",
                    BasePrice = 2999, IsFeatured = true,
                    Images = new List<ProductImage>
                    {
                        new() { Url = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=600", DisplayOrder = 0 },
                        new() { Url = "https://images.unsplash.com/photo-1608231387042-66d1773070a5?w=600", DisplayOrder = 1 }
                    },
                    Variants = new List<ProductVariant>
                    {
                        new() { Sku = "SX-AMR-BLK-8", Color = "Black/Red", ColorHex = "#1a1a2e", Size = "8UK", Price = 2499, Mrp = 3999, Stock = 25 },
                        new() { Sku = "SX-AMR-BLK-9", Color = "Black/Red", ColorHex = "#1a1a2e", Size = "9UK", Price = 2499, Mrp = 3999, Stock = 30 },
                        new() { Sku = "SX-AMR-WHT-9", Color = "White/Blue", ColorHex = "#f0f0f0", Size = "9UK", Price = 2499, Mrp = 3999, Stock = 20 },
                        new() { Sku = "SX-AMR-WHT-10", Color = "White/Blue", ColorHex = "#f0f0f0", Size = "10UK", Price = 2499, Mrp = 3999, Stock = 15 },
                    }
                },
                new()
                {
                    Title = "Chronograph Steel Watch", Slug = "chronograph-steel-watch",
                    Description = "Premium stainless steel chronograph watch with sapphire crystal glass. Japanese quartz movement. 50m water resistance.",
                    CategoryId = watchesId, Brand = "TimeCraft", Gender = "men", Material = "Stainless Steel",
                    BasePrice = 4999, IsFeatured = true,
                    Images = new List<ProductImage>
                    {
                        new() { Url = "https://images.unsplash.com/photo-1524592094714-0f0654e20314?w=600", DisplayOrder = 0 }
                    },
                    Variants = new List<ProductVariant>
                    {
                        new() { Sku = "TC-CHR-SLV-OS", Color = "Silver", ColorHex = "#C0C0C0", Size = "One Size", Price = 3999, Mrp = 6999, Stock = 15 },
                        new() { Sku = "TC-CHR-GLD-OS", Color = "Gold", ColorHex = "#FFD700", Size = "One Size", Price = 4499, Mrp = 7499, Stock = 10 },
                        new() { Sku = "TC-CHR-BLK-OS", Color = "Black", ColorHex = "#1a1a1a", Size = "One Size", Price = 4299, Mrp = 7299, Stock = 12 },
                    }
                },
                new()
                {
                    Title = "Leather Crossbody Sling Bag", Slug = "leather-crossbody-sling-bag",
                    Description = "Genuine leather sling bag with adjustable strap. Multiple compartments with magnetic snap closure. Perfect for daily essentials.",
                    CategoryId = bags, Brand = "LeatherLux", Gender = "women", Material = "Genuine Leather",
                    BasePrice = 1899, IsFeatured = true,
                    Images = new List<ProductImage>
                    {
                        new() { Url = "https://images.unsplash.com/photo-1548036328-c9fa89d128fa?w=600", DisplayOrder = 0 }
                    },
                    Variants = new List<ProductVariant>
                    {
                        new() { Sku = "LL-CSB-TAN-OS", Color = "Tan", ColorHex = "#D2B48C", Size = "One Size", Price = 1599, Mrp = 2499, Stock = 20 },
                        new() { Sku = "LL-CSB-BLK-OS", Color = "Black", ColorHex = "#000000", Size = "One Size", Price = 1599, Mrp = 2499, Stock = 25 },
                        new() { Sku = "LL-CSB-BRG-OS", Color = "Burgundy", ColorHex = "#800020", Size = "One Size", Price = 1699, Mrp = 2599, Stock = 15 },
                    }
                }
            };

            db.Products.AddRange(products);
            await db.SaveChangesAsync();

            // === COUPONS ===
            db.Coupons.AddRange(
                new Coupon { Code = "WELCOME50", Type = "flat", Value = 50, MinOrderAmount = 299, MaxDiscount = 50, UsageLimit = 1000, PerUserLimit = 1, ValidFrom = DateTime.UtcNow.AddDays(-1), ValidUntil = DateTime.UtcNow.AddMonths(6) },
                new Coupon { Code = "FLAT20", Type = "percentage", Value = 20, MinOrderAmount = 999, MaxDiscount = 500, UsageLimit = 500, PerUserLimit = 3, ValidFrom = DateTime.UtcNow.AddDays(-1), ValidUntil = DateTime.UtcNow.AddMonths(3) },
                new Coupon { Code = "FASHION100", Type = "flat", Value = 100, MinOrderAmount = 599, MaxDiscount = 100, UsageLimit = 200, PerUserLimit = 2, ValidFrom = DateTime.UtcNow.AddDays(-1), ValidUntil = DateTime.UtcNow.AddMonths(2) },
                new Coupon { Code = "MEGA30", Type = "percentage", Value = 30, MinOrderAmount = 1999, MaxDiscount = 1000, UsageLimit = 100, PerUserLimit = 1, ValidFrom = DateTime.UtcNow.AddDays(-1), ValidUntil = DateTime.UtcNow.AddMonths(1) }
            );
            await db.SaveChangesAsync();
        }
    }
}
