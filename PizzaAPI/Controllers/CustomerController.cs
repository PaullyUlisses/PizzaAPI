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
public class CustomerController : ControllerBase
{
    private readonly PizzaDbContext _context;

    public CustomerController(PizzaDbContext context)
    {
        _context = context;
    }

    // Admin only: Get all customers
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        var customers = await _context.Customers.ToListAsync();
        return Ok(customers.Select(MapToDto));
    }

    // Get own profile or any profile if admin
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        var currentUserId = GetCurrentUserId();
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Users can only view their own profile, admins can view any
        if (currentUserRole != "Admin" && currentUserId != id)
        {
            return Forbid();
        }

        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(customer));
    }

    // Update own profile or any profile if admin
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCustomer(int id, CustomerUpdateDto customerDto)
    {
        if (id != customerDto.Id)
        {
            return BadRequest();
        }

        var currentUserId = GetCurrentUserId();
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Users can only update their own profile, admins can update any
        if (currentUserRole != "Admin" && currentUserId != id)
        {
            return Forbid();
        }

        var existingCustomer = await _context.Customers.FindAsync(id);
        if (existingCustomer == null)
        {
            return NotFound();
        }

        // Check if email is being changed to one that already exists
        if (existingCustomer.Email != customerDto.Email)
        {
            var emailExists = await _context.Customers
                .AnyAsync(c => c.Email == customerDto.Email && c.Id != id);

            if (emailExists)
            {
                return BadRequest(new { message = "Email already exists" });
            }
        }

        existingCustomer.FirstName = customerDto.FirstName;
        existingCustomer.LastName = customerDto.LastName;
        existingCustomer.Email = customerDto.Email;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CustomerExists(id))
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

    // Admin only: Delete customer
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out int userId) ? userId : 0;
    }

    private bool CustomerExists(int id)
    {
        return _context.Customers.Any(e => e.Id == id);
    }

    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Role = customer.Role,
            OrderAmount = customer.OrderAmount
        };
    }
}
