using System.ComponentModel.DataAnnotations;

namespace PizzaAPI.DTOs;

public class OrderItemDto
{
    [Required]
    public int Quantity { get; set; }

    [Required]
    public int PizzaId { get; set; }
}

public class OrderItemUpdateDto : OrderItemDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
}