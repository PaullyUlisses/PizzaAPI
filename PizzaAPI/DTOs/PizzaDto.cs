using System.ComponentModel.DataAnnotations;
using PizzaAPI.Models;

namespace PizzaAPI.DTOs;

public class PizzaCreateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public decimal Price { get; set; }

    [Required]
    public PizzaBase Base { get; set; }

    public List<PizzaIngredientMappingDto> Ingredients { get; set; } = new List<PizzaIngredientMappingDto>();
}

public class PizzaUpdateDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public decimal Price { get; set; }

    [Required]
    public PizzaBase Base { get; set; }

    public List<PizzaIngredientMappingUpdateDto> Ingredients { get; set; } = new List<PizzaIngredientMappingUpdateDto>();
}