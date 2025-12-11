using System.ComponentModel.DataAnnotations;

namespace Products.Api.Models.DTOs;

public class CreateProductDto
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
    public decimal Price { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be non-negative")]
    public int StockQuantity { get; set; } = 0;
}



