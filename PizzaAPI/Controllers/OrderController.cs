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
        // Validate that all pizzas exist and get their current prices
        var pizzaIds = orderDto.OrderItems.Select(item => item.PizzaId).Distinct().ToList();
        var pizzas = await _context.Pizzas
            .Where(p => pizzaIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Price);

        // Check if all pizzas exist
        foreach (var itemDto in orderDto.OrderItems)
        {
            if (!pizzas.ContainsKey(itemDto.PizzaId))
            {
                return BadRequest($"Pizza with ID {itemDto.PizzaId} does not exist.");
            }
        }

        var order = new Order
        {
            OrderDate = orderDto.OrderDate ?? DateTime.UtcNow,
            Status = orderDto.Status ?? OrderStatus.Pending,
            CustomerId = orderDto.CustomerId
        };

        decimal calculatedTotal = 0;

        foreach (var itemDto in orderDto.OrderItems)
        {
            var pizzaPrice = pizzas[itemDto.PizzaId];
            var itemTotal = pizzaPrice * itemDto.Quantity;
            calculatedTotal += itemTotal;

            var orderItem = new OrderItem
            {
                Quantity = itemDto.Quantity,
                Price = pizzaPrice,
                PizzaId = itemDto.PizzaId,
                Order = order
            };
            order.OrderItems.Add(orderItem);
        }

        // Set the calculated total price
        order.TotalPrice = calculatedTotal;

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

        // Validate that all pizzas exist and get their current prices
        var pizzaIds = orderDto.OrderItems.Select(item => item.PizzaId).Distinct().ToList();
        var pizzas = await _context.Pizzas
            .Where(p => pizzaIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Price);

        // Check if all pizzas exist
        foreach (var itemDto in orderDto.OrderItems)
        {
            if (!pizzas.ContainsKey(itemDto.PizzaId))
            {
                return BadRequest($"Pizza with ID {itemDto.PizzaId} does not exist.");
            }
        }

        existingOrder.OrderDate = orderDto.OrderDate;
        existingOrder.Status = orderDto.Status;
        existingOrder.CustomerId = orderDto.CustomerId;

        // Remove existing order items
        _context.OrderItems.RemoveRange(existingOrder.OrderItems);

        decimal calculatedTotal = 0;

        foreach (var itemDto in orderDto.OrderItems)
        {
            var pizzaPrice = pizzas[itemDto.PizzaId];
            var itemTotal = pizzaPrice * itemDto.Quantity;
            calculatedTotal += itemTotal;

            var orderItem = new OrderItem
            {
                Quantity = itemDto.Quantity,
                Price = pizzaPrice,
                PizzaId = itemDto.PizzaId,
                OrderId = existingOrder.Id
            };
            existingOrder.OrderItems.Add(orderItem);
        }

        // Set the calculated total price
        existingOrder.TotalPrice = calculatedTotal;

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

    [HttpPatch("status/{id}")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        // Validate cancellation rules
        if (status == OrderStatus.Cancelled && 
            order.Status != OrderStatus.Pending && 
            order.Status != OrderStatus.Confirmed)
        {
            return BadRequest($"Orders can only be cancelled when in 'Pending' or 'Confirmed' status. Current status: {order.Status}");
        }

        order.Status = status;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("confirm/{id}")]
    public async Task<IActionResult> ConfirmOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        if (order.Status != OrderStatus.Pending)
        {
            return BadRequest($"Only pending orders can be confirmed. Current status: {order.Status}");
        }

        order.Status = OrderStatus.Confirmed;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("cancel/{id}")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
        {
            return BadRequest($"Orders can only be cancelled when in 'Pending' or 'Confirmed' status. Current status: {order.Status}");
        }

        order.Status = OrderStatus.Cancelled;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("start/{id}")]
    public async Task<IActionResult> StartOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        if (order.Status != OrderStatus.Confirmed)
        {
            return BadRequest($"Only confirmed orders can be started. Current status: {order.Status}");
        }

        order.Status = OrderStatus.InProgress;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("ready/{id}")]
    public async Task<IActionResult> MarkOrderReady(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        if (order.Status != OrderStatus.InProgress)
        {
            return BadRequest($"Only in-progress orders can be marked as ready. Current status: {order.Status}");
        }

        order.Status = OrderStatus.Ready;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("deliver/{id}")]
    public async Task<IActionResult> DeliverOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        if (order.Status != OrderStatus.Ready)
        {
            return BadRequest($"Only ready orders can be delivered. Current status: {order.Status}");
        }

        order.Status = OrderStatus.Delivered;
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