using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzaAPI.Models;

public class Pizza
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(8,2)")]
    public decimal Price { get; set; }

    [Required]
    public PizzaBase Base { get; set; }

    public virtual ICollection<PizzaIngredientMapping> Ingredients { get; set; } = new List<PizzaIngredientMapping>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}