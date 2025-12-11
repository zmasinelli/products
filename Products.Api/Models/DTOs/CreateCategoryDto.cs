using System.ComponentModel.DataAnnotations;

namespace Products.Api.Models.DTOs;

public class CreateCategoryDto
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}



