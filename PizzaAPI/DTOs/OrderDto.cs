using System.ComponentModel.DataAnnotations;
using PizzaAPI.Models;

namespace PizzaAPI.DTOs;

public class OrderCreateDto
{
    public DateTime? OrderDate { get; set; }
    
    public OrderStatus? Status { get; set; }

    [Required]
    public int CustomerId { get; set; }

    public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
}

public class OrderUpdateDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    public DateTime OrderDate { get; set; }

    [Required]
    public OrderStatus Status { get; set; }

    [Required]
    public int CustomerId { get; set; }

    public List<OrderItemUpdateDto> OrderItems { get; set; } = new List<OrderItemUpdateDto>();
}