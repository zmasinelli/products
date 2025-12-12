using System.ComponentModel.DataAnnotations;

namespace Products.Api.Models.DTOs;

public class UpdateProductDto
{
    [StringLength(255)]
    public string? Name { get; set; }

    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal? Price { get; set; }

    public int? CategoryId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be non-negative")]
    public int? StockQuantity { get; set; }

    public bool? IsActive { get; set; }
}



