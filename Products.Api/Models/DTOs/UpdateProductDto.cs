using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Products.Api.Models.DTOs;

public class UpdateProductDto
{
    [StringLength(255)]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("categoryId")]
    public int? CategoryId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be non-negative")]
    [JsonPropertyName("stockQuantity")]
    public int? StockQuantity { get; set; }

    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }
}



