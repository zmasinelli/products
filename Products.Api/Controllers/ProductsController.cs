using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Products.Api.Data;
using Products.Api.Models;
using Products.Api.Models.DTOs;

namespace Products.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var products = await _context.Products
            .Where(p => p.IsActive)
            .Include(p => p.Category)
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

        return Ok(products);
    }

    // GET: api/products/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _context.Products
            .Where(p => p.Id == id && p.IsActive)
            .Include(p => p.Category)
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

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validate category exists and is active
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == createDto.CategoryId && c.IsActive);

        if (category == null)
        {
            return BadRequest("Category not found or inactive.");
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

        var productDto = new ProductDto
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

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
    }

    // PUT: api/products/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
        {
            return NotFound();
        }

        // Validate category if provided
        if (updateDto.CategoryId.HasValue)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == updateDto.CategoryId.Value && c.IsActive);

            if (category == null)
            {
                return BadRequest("Category not found or inactive.");
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

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProductExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/products/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
        {
            return NotFound();
        }

        // Soft delete
        product.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> ProductExists(int id)
    {
        return await _context.Products.AnyAsync(e => e.Id == id);
    }
}



