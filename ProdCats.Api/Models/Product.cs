using System.ComponentModel.DataAnnotations.Schema;

namespace ProdCats.Api.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }
    
    public int CategoryId { get; set; }
    public int StockQuantity { get; set; } = 0;
    
    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;

    // Navigation property
    public Category Category { get; set; } = null!;
}



