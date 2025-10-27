using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzaAPI.Models;

public class PizzaIngredientMapping
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PizzaId { get; set; }

    [Required]
    public PizzaIngredient Ingredient { get; set; }

    [ForeignKey("PizzaId")]
    public virtual Pizza Pizza { get; set; } = null!;
}