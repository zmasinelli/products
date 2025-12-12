using Microsoft.EntityFrameworkCore;
using ProdCats.Api.Models;

namespace ProdCats.Api.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if data already exists
        if (await context.Categories.AnyAsync())
        {
            return; // Data already seeded
        }

        // Seed Categories
        var categories = new List<Category>
        {
            new Category
            {
                Name = "Electronics",
                Description = "Electronic devices and gadgets",
                IsActive = true
            },
            new Category
            {
                Name = "Clothing",
                Description = "Apparel and fashion items",
                IsActive = true
            },
            new Category
            {
                Name = "Home & Garden",
                Description = "Home improvement and garden supplies",
                IsActive = true
            },
            new Category
            {
                Name = "Sports & Outdoors",
                Description = "Sports equipment and outdoor gear",
                IsActive = true
            },
            new Category
            {
                Name = "Books",
                Description = "Books and reading materials",
                IsActive = true
            },
            new Category 
            {
                Name = "Toys - Inactive",
                Description = "Toys and games",
                IsActive = false
            }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // Seed Products
        var products = new List<Product>
        {
            // Electronics
            new Product
            {
                Name = "Wireless Bluetooth Headphones",
                Description = "Premium noise-cancelling headphones with 30-hour battery life",
                Price = 199.99m,
                CategoryId = categories[0].Id,
                StockQuantity = 45,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-30)
            },
            new Product
            {
                Name = "Smartphone 128GB",
                Description = "Latest generation smartphone with advanced camera system",
                Price = 899.99m,
                CategoryId = categories[0].Id,
                StockQuantity = 12,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-15)
            },
            new Product
            {
                Name = "4K Ultra HD TV 55\"",
                Description = "55-inch 4K smart TV with HDR support",
                Price = 649.99m,
                CategoryId = categories[0].Id,
                StockQuantity = 8,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-20)
            },
            new Product
            {
                Name = "Laptop 16GB RAM",
                Description = "High-performance laptop for work and gaming",
                Price = 1299.99m,
                CategoryId = categories[0].Id,
                StockQuantity = 5,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-10)
            },
            new Product
            {
                Name = "Smart Watch",
                Description = "Fitness tracker with heart rate monitor and GPS",
                Price = 249.99m,
                CategoryId = categories[0].Id,
                StockQuantity = 0,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-5)
            },

            // Clothing
            new Product
            {
                Name = "Cotton T-Shirt",
                Description = "100% organic cotton t-shirt, available in multiple colors",
                Price = 24.99m,
                CategoryId = categories[1].Id,
                StockQuantity = 150,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-25)
            },
            new Product
            {
                Name = "Denim Jeans",
                Description = "Classic fit denim jeans, multiple sizes available",
                Price = 79.99m,
                CategoryId = categories[1].Id,
                StockQuantity = 87,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-22)
            },
            new Product
            {
                Name = "Winter Jacket",
                Description = "Waterproof winter jacket with insulated lining",
                Price = 149.99m,
                CategoryId = categories[1].Id,
                StockQuantity = 23,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-18)
            },
            new Product
            {
                Name = "Running Shoes",
                Description = "Lightweight running shoes with cushioned sole",
                Price = 89.99m,
                CategoryId = categories[1].Id,
                StockQuantity = 34,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-12)
            },

            // Home & Garden
            new Product
            {
                Name = "Garden Tool Set",
                Description = "Complete set of essential gardening tools",
                Price = 59.99m,
                CategoryId = categories[2].Id,
                StockQuantity = 28,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-28)
            },
            new Product
            {
                Name = "Indoor Plant Pot Set",
                Description = "Set of 3 ceramic plant pots in various sizes",
                Price = 34.99m,
                CategoryId = categories[2].Id,
                StockQuantity = 56,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-14)
            },
            new Product
            {
                Name = "LED String Lights",
                Description = "50ft weatherproof LED string lights for outdoor use",
                Price = 19.99m,
                CategoryId = categories[2].Id,
                StockQuantity = 92,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-8)
            },
            new Product
            {
                Name = "Coffee Maker",
                Description = "Programmable coffee maker with thermal carafe",
                Price = 79.99m,
                CategoryId = categories[2].Id,
                StockQuantity = 15,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-16)
            },

            // Sports & Outdoors
            new Product
            {
                Name = "Yoga Mat",
                Description = "Non-slip yoga mat with carrying strap",
                Price = 29.99m,
                CategoryId = categories[3].Id,
                StockQuantity = 67,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-19)
            },
            new Product
            {
                Name = "Camping Tent 4-Person",
                Description = "Weather-resistant 4-person camping tent",
                Price = 199.99m,
                CategoryId = categories[3].Id,
                StockQuantity = 11,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-11)
            },
            new Product
            {
                Name = "Bicycle Helmet",
                Description = "Safety-certified bicycle helmet with adjustable fit",
                Price = 49.99m,
                CategoryId = categories[3].Id,
                StockQuantity = 38,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-7)
            },
            new Product
            {
                Name = "Dumbbell Set 20lb",
                Description = "Adjustable dumbbell set, 5-20 pounds per dumbbell",
                Price = 129.99m,
                CategoryId = categories[3].Id,
                StockQuantity = 9,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-13)
            },

            // Books
            new Product
            {
                Name = "Programming Fundamentals",
                Description = "Comprehensive guide to programming concepts",
                Price = 39.99m,
                CategoryId = categories[4].Id,
                StockQuantity = 42,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-21)
            },
            new Product
            {
                Name = "Mystery Novel",
                Description = "Bestselling mystery thriller novel",
                Price = 14.99m,
                CategoryId = categories[4].Id,
                StockQuantity = 78,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-9)
            },
            new Product
            {
                Name = "Cookbook Collection",
                Description = "Set of 3 cookbooks with 500+ recipes",
                Price = 49.99m,
                CategoryId = categories[4].Id,
                StockQuantity = 19,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-6)
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}
