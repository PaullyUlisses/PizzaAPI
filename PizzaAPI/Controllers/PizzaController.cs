using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaAPI.Data;
using PizzaAPI.Models;
using PizzaAPI.DTOs;

namespace PizzaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PizzaController : ControllerBase
{
    private readonly PizzaDbContext _context;

    public PizzaController(PizzaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pizza>>> GetPizzas()
    {
        return await _context.Pizzas
            .Include(p => p.Ingredients)
            .Include(p => p.OrderItems)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Pizza>> GetPizza(int id)
    {
        var pizza = await _context.Pizzas
            .Include(p => p.Ingredients)
            .Include(p => p.OrderItems)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pizza == null)
        {
            return NotFound();
        }

        return pizza;
    }

    [HttpGet("by-base/{baseType}")]
    public async Task<ActionResult<IEnumerable<Pizza>>> GetPizzasByBase(PizzaBase baseType)
    {
        return await _context.Pizzas
            .Where(p => p.Base == baseType)
            .Include(p => p.Ingredients)
            .ToListAsync();
    }

    [HttpGet("by-price-range")]
    public async Task<ActionResult<IEnumerable<Pizza>>> GetPizzasByPriceRange([FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
    {
        var query = _context.Pizzas.AsQueryable();

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        return await query
            .Include(p => p.Ingredients)
            .ToListAsync();
    }

    // Admin only: Create pizza
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Pizza>> PostPizza(PizzaCreateDto pizzaDto)
    {
        var pizza = new Pizza
        {
            Name = pizzaDto.Name,
            Price = pizzaDto.Price,
            Base = pizzaDto.Base
        };

        foreach (var ingredientDto in pizzaDto.Ingredients)
        {
            var ingredientMapping = new PizzaIngredientMapping
            {
                Ingredient = ingredientDto.Ingredient,
                Pizza = pizza
            };
            pizza.Ingredients.Add(ingredientMapping);
        }

        _context.Pizzas.Add(pizza);
        await _context.SaveChangesAsync();

        var createdPizza = await _context.Pizzas
            .Include(p => p.Ingredients)
            .FirstOrDefaultAsync(p => p.Id == pizza.Id);

        return CreatedAtAction(nameof(GetPizza), new { id = pizza.Id }, createdPizza);
    }

    // Admin only: Update pizza
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutPizza(int id, PizzaUpdateDto pizzaDto)
    {
        if (id != pizzaDto.Id)
        {
            return BadRequest();
        }

        var existingPizza = await _context.Pizzas
            .Include(p => p.Ingredients)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (existingPizza == null)
        {
            return NotFound();
        }

        existingPizza.Name = pizzaDto.Name;
        existingPizza.Price = pizzaDto.Price;
        existingPizza.Base = pizzaDto.Base;

        _context.PizzaIngredientMappings.RemoveRange(existingPizza.Ingredients);

        foreach (var ingredientDto in pizzaDto.Ingredients)
        {
            var ingredientMapping = new PizzaIngredientMapping
            {
                Ingredient = ingredientDto.Ingredient,
                PizzaId = existingPizza.Id
            };
            existingPizza.Ingredients.Add(ingredientMapping);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PizzaExists(id))
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

    // Admin only: Update pizza price
    [HttpPut("{id}/price")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePizzaPrice(int id, [FromBody] decimal price)
    {
        var pizza = await _context.Pizzas.FindAsync(id);
        if (pizza == null)
        {
            return NotFound();
        }

        if (price <= 0)
        {
            return BadRequest("Price must be greater than 0");
        }

        pizza.Price = price;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Admin only: Delete pizza
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePizza(int id)
    {
        var pizza = await _context.Pizzas.FindAsync(id);
        if (pizza == null)
        {
            return NotFound();
        }

        _context.Pizzas.Remove(pizza);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PizzaExists(int id)
    {
        return _context.Pizzas.Any(e => e.Id == id);
    }
}