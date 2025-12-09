using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaAPI.Data;
using PizzaAPI.DTOs;
using PizzaAPI.Models;
using System.Security.Claims;

namespace PizzaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class OrderController : ControllerBase
{
    private readonly PizzaDbContext _context;

    public OrderController(PizzaDbContext context)
    {
        _context = context;
    }

    // Admin only: Get all orders
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Pizza)
            .ToListAsync();
    }

    // Get order by ID - Users can only view their own orders
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

        // Users can only view their own orders, admins can view any
        var currentUserId = OrderControllerHelper.GetCurrentUserId(this.User);
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (currentUserRole != "Admin" && order.CustomerId != currentUserId)
        {
            return Forbid();
        }

        return order;
    }

    // Get orders by customer - Users can only view their own orders
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByCustomer(int customerId)
    {
        var currentUserId = OrderControllerHelper.GetCurrentUserId(this.User);
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Users can only view their own orders
        if (currentUserRole != "Admin" && currentUserId != customerId)
        {
            return Forbid();
        }

        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Pizza)
            .ToListAsync();
    }

    // POST /api/order/orders - Create order for authenticated user
    [HttpPost("orders")]
    public async Task<ActionResult<Order>> CreateMyOrder(OrderCreateDto orderDto)
    {
        var currentUserId = OrderControllerHelper.GetCurrentUserId(this.User);

        // Automatically set the customer ID from JWT - ignore any provided customerId
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
                return BadRequest(new { message = $"Pizza with ID {itemDto.PizzaId} does not exist." });
            }
        }

        var order = new Order
        {
            OrderDate = orderDto.OrderDate ?? DateTime.UtcNow,
            Status = orderDto.Status ?? OrderStatus.Pending,
            CustomerId = currentUserId // Always use authenticated user's ID
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

    // Create order - Users can only create orders for themselves
    [HttpPost]
    public async Task<ActionResult<Order>> PostOrder(OrderCreateDto orderDto)
    {
        var currentUserId = OrderControllerHelper.GetCurrentUserId(this.User);
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Users can only create orders for themselves
        if (currentUserRole != "Admin" && orderDto.CustomerId != currentUserId)
        {
            return Forbid();
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

    // Admin only: Update order
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
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
            if (!OrderControllerHelper.OrderExists(id, _context))
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

    // Admin only: Update order status
    [HttpPatch("status/{id}")]
    [Authorize(Roles = "Admin")]
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

    // Admin only: Confirm order
    [HttpPut("confirm/{id}")]
    [Authorize(Roles = "Admin")]
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

    // Admin only: Cancel order
    [HttpPut("cancel/{id}")]
    [Authorize(Roles = "Admin")]
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

    // Admin only: Start order
    [HttpPut("start/{id}")]
    [Authorize(Roles = "Admin")]
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

    // Admin only: Mark order as ready
    [HttpPut("ready/{id}")]
    [Authorize(Roles = "Admin")]
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

    // Admin only: Deliver order
    [HttpPut("deliver/{id}")]
    [Authorize(Roles = "Admin")]
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

    // Admin only: Delete order
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
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
}

[ApiController]
[Route("api/my")]
[Authorize] // All endpoints require authentication
public class MyOrderController : ControllerBase
{
    private readonly PizzaDbContext _context;

    public MyOrderController(PizzaDbContext context)
    {
        _context = context;
    }

    // GET /api/my/orders - Get all orders for authenticated user
    [HttpGet("orders")]
    public async Task<ActionResult<IEnumerable<Order>>> GetMyOrders()
    {
        var currentUserId = OrderControllerHelper.GetCurrentUserId(this.User);

        return await _context.Orders
            .Where(o => o.CustomerId == currentUserId)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Pizza)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    // GET /api/my/orders/{id} - Get specific order for authenticated user
    [HttpGet("orders/{id}")]
    public async Task<ActionResult<Order>> GetMyOrder(int id)
    {
        var currentUserId = OrderControllerHelper.GetCurrentUserId(this.User);

        var order = await _context.Orders
            .Where(o => o.CustomerId == currentUserId && o.Id == id)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Pizza)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return NotFound(new { message = "Order not found or does not belong to you" });
        }

        return order;
    }

    // PUT /api/my/orders/{id} - Update order for authenticated user
    [HttpPut("orders/{id}")]
    public async Task<IActionResult> UpdateMyOrder(int id, OrderUpdateDto orderDto)
    {
        if (id != orderDto.Id)
        {
            return BadRequest(new { message = "Order ID mismatch" });
        }

        var currentUserId = OrderControllerHelper.GetCurrentUserId(this.User);

        var existingOrder = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == currentUserId && o.Id == id)
            .FirstOrDefaultAsync();

        if (existingOrder == null)
        {
            return NotFound(new { message = "Order not found or does not belong to you" });
        }

        // Don't allow changing order if it's already in progress or delivered
        if (existingOrder.Status >= OrderStatus.InProgress)
        {
            return BadRequest(new { message = $"Cannot modify order with status '{existingOrder.Status}'. Only Pending or Confirmed orders can be modified." });
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
                return BadRequest(new { message = $"Pizza with ID {itemDto.PizzaId} does not exist." });
            }
        }

        existingOrder.OrderDate = orderDto.OrderDate;
        existingOrder.Status = orderDto.Status;
        // CustomerId is NOT updated - it stays with the original customer

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
            if (!OrderControllerHelper.OrderExists(id, _context))
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
}

internal static class OrderControllerHelper
{
    internal static int GetCurrentUserId(ClaimsPrincipal claims)
    {
        var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out int userId) ? userId : 0;
    }

    internal static bool OrderExists(int id, PizzaDbContext context)
    {
        return context.Orders.Any(e => e.Id == id);
    }
}