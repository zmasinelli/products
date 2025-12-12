using Products.Api.Models.DTOs;

namespace Products.Api.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto);
    Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto updateDto);
}
