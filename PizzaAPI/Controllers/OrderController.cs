using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaAPI.Data;
using PizzaAPI.Models;
using PizzaAPI.DTOs;

namespace PizzaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly PizzaDbContext _context;

    public OrderController(PizzaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Pizza)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Pizza)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return order;
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByCustomer(int customerId)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Pizza)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Order>> PostOrder(OrderCreateDto orderDto)
    {
        var order = new Order
        {
            OrderDate = orderDto.OrderDate ?? DateTime.UtcNow,
            Status = orderDto.Status ?? OrderStatus.Pending,
            TotalPrice = orderDto.TotalPrice,
            CustomerId = orderDto.CustomerId
        };

        foreach (var itemDto in orderDto.OrderItems)
        {
            var orderItem = new OrderItem
            {
                Quantity = itemDto.Quantity,
                Price = itemDto.Price,
                PizzaId = itemDto.PizzaId,
                Order = order
            };
            order.OrderItems.Add(orderItem);
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var createdOrder = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Pizza)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, createdOrder);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutOrder(int id, OrderUpdateDto orderDto)
    {
        if (id != orderDto.Id)
        {
            return BadRequest();
        }

        var existingOrder = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (existingOrder == null)
        {
            return NotFound();
        }

        existingOrder.OrderDate = orderDto.OrderDate;
        existingOrder.Status = orderDto.Status;
        existingOrder.TotalPrice = orderDto.TotalPrice;
        existingOrder.CustomerId = orderDto.CustomerId;

        _context.OrderItems.RemoveRange(existingOrder.OrderItems);

        foreach (var itemDto in orderDto.OrderItems)
        {
            var orderItem = new OrderItem
            {
                Quantity = itemDto.Quantity,
                Price = itemDto.Price,
                PizzaId = itemDto.PizzaId,
                OrderId = existingOrder.Id
            };
            existingOrder.OrderItems.Add(orderItem);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!OrderExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = status;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool OrderExists(int id)
    {
        return _context.Orders.Any(e => e.Id == id);
    }
}