using Products.Api.Models.DTOs;

namespace Products.Api.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync();
    Task<SearchProductsResponseDto> SearchProductsAsync(
        string? searchTerm = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? inStock = null,
        string? sortBy = null,
        string? sortOrder = null,
        int pageNumber = 1,
        int pageSize = 10);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductDto createDto);
    Task<bool> UpdateProductAsync(int id, UpdateProductDto updateDto);
    Task<bool> DeleteProductAsync(int id);
}
