using System.ComponentModel.DataAnnotations;
using PizzaAPI.Models;

namespace PizzaAPI.DTOs;

public class PizzaIngredientMappingDto
{
    [Required]
    public PizzaIngredient Ingredient { get; set; }
}

public class PizzaIngredientMappingUpdateDto : PizzaIngredientMappingDto
{
    public int Id { get; set; }
    public int PizzaId { get; set; }
}