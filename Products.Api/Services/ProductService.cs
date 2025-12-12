using Microsoft.EntityFrameworkCore;
using Products.Api.Data;
using Products.Api.Models;
using Products.Api.Models.DTOs;

namespace Products.Api.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync()
    {
        return await _context.Products
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .AsNoTracking()
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                StockQuantity = p.StockQuantity,
                CreatedDate = p.CreatedDate,
                IsActive = p.IsActive
            })
            .ToListAsync();
    }

    public async Task<SearchProductsResponseDto> SearchProductsAsync(
        string? searchTerm = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? inStock = null,
        string? sortBy = null,
        string? sortOrder = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        // Build base query - only active products
        var query = _context.Products
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .AsNoTracking()
            .AsQueryable();

        // Apply searchTerm filter with AND logic
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchWords = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in searchWords)
            {
                var searchPattern = $"%{word}%";
                query = query.Where(p =>
                    EF.Functions.ILike(p.Name, searchPattern) ||
                    (p.Description != null && EF.Functions.ILike(p.Description, searchPattern)));
            }
        }

        // Apply categoryId filter
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // Apply price range filters
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        // Apply inStock filter
        if (inStock.HasValue)
        {
            if (inStock.Value)
            {
                query = query.Where(p => p.StockQuantity > 0);
            }
            else
            {
                query = query.Where(p => p.StockQuantity == 0);
            }
        }

        // Apply sorting
        sortOrder = sortOrder?.ToLower() ?? "asc";
        var isDescending = sortOrder == "desc";

        query = (sortBy?.ToLower()) switch
        {
            "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => isDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "createddate" => isDescending ? query.OrderByDescending(p => p.CreatedDate) : query.OrderBy(p => p.CreatedDate),
            _ => query.OrderBy(p => p.Id) // Default sort
        };

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var skip = (pageNumber - 1) * pageSize;
        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                StockQuantity = p.StockQuantity,
                CreatedDate = p.CreatedDate,
                IsActive = p.IsActive
            })
            .ToListAsync();

        // Calculate total pages
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new SearchProductsResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        return await _context.Products
            .Where(p => p.Id == id && p.IsActive)
            .Include(p => p.Category)
            .AsNoTracking()
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                StockQuantity = p.StockQuantity,
                CreatedDate = p.CreatedDate,
                IsActive = p.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto)
    {
        // Validate category exists and is active
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == createDto.CategoryId && c.IsActive);

        if (category == null)
        {
            throw new InvalidOperationException("Category not found or inactive.");
        }

        var product = new Product
        {
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price,
            CategoryId = createDto.CategoryId,
            StockQuantity = createDto.StockQuantity,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = category.Name,
            StockQuantity = product.StockQuantity,
            CreatedDate = product.CreatedDate,
            IsActive = product.IsActive
        };
    }

    public async Task<bool> UpdateProductAsync(int id, UpdateProductDto updateDto)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return false;
        }

        // Validate category if provided
        if (updateDto.CategoryId.HasValue)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == updateDto.CategoryId.Value && c.IsActive);

            if (category == null)
            {
                throw new InvalidOperationException("Category not found or inactive.");
            }

            product.CategoryId = updateDto.CategoryId.Value;
        }

        // Update only provided fields
        if (updateDto.Name != null)
        {
            product.Name = updateDto.Name;
        }

        if (updateDto.Description != null)
        {
            product.Description = updateDto.Description;
        }

        if (updateDto.Price.HasValue)
        {
            product.Price = updateDto.Price.Value;
        }

        if (updateDto.StockQuantity.HasValue)
        {
            product.StockQuantity = updateDto.StockQuantity.Value;
        }

        if (updateDto.IsActive.HasValue)
        {
            product.IsActive = updateDto.IsActive.Value;
        }

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProductExists(id))
            {
                return false;
            }
            throw;
        }
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
        {
            return false;
        }

        // Soft delete
        product.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<bool> ProductExists(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .AnyAsync(e => e.Id == id);
    }
}
