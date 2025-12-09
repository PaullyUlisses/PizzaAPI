using PizzaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace PizzaAPI.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(PizzaDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (!await context.Pizzas.AnyAsync())
        {
            var pizzas = new List<Pizza>
            {
                new Pizza
                {
                    Name = "Margherita",
                    Price = 12.99m,
                    Base = PizzaBase.Tomato
                },
                new Pizza
                {
                    Name = "Pepperoni",
                    Price = 14.99m,
                    Base = PizzaBase.Tomato
                },
                new Pizza
                {
                    Name = "Hawaiian",
                    Price = 15.99m,
                    Base = PizzaBase.Cream
                },
                new Pizza
                {
                    Name = "Meat Lovers",
                    Price = 18.99m,
                    Base = PizzaBase.Tomato
                },
                new Pizza
                {
                    Name = "Veggie Supreme",
                    Price = 16.99m,
                    Base = PizzaBase.Tomato
                },
                new Pizza
                {
                    Name = "BBQ Chicken",
                    Price = 17.99m,
                    Base = PizzaBase.Cream
                }
            };

            await context.Pizzas.AddRangeAsync(pizzas);
            await context.SaveChangesAsync();

            var pizzaIngredients = new List<PizzaIngredientMapping>
            {
                // Margherita
                new PizzaIngredientMapping { PizzaId = 1, Ingredient = PizzaIngredient.Mozzarella },
                new PizzaIngredientMapping { PizzaId = 1, Ingredient = PizzaIngredient.Basil },

                // Pepperoni
                new PizzaIngredientMapping { PizzaId = 2, Ingredient = PizzaIngredient.Mozzarella },
                new PizzaIngredientMapping { PizzaId = 2, Ingredient = PizzaIngredient.Pepperoni },

                // Hawaiian
                new PizzaIngredientMapping { PizzaId = 3, Ingredient = PizzaIngredient.Mozzarella },
                new PizzaIngredientMapping { PizzaId = 3, Ingredient = PizzaIngredient.Ham },
                new PizzaIngredientMapping { PizzaId = 3, Ingredient = PizzaIngredient.Pineapple },

                // Meat Lovers
                new PizzaIngredientMapping { PizzaId = 4, Ingredient = PizzaIngredient.Mozzarella },
                new PizzaIngredientMapping { PizzaId = 4, Ingredient = PizzaIngredient.Pepperoni },
                new PizzaIngredientMapping { PizzaId = 4, Ingredient = PizzaIngredient.Sausage },
                new PizzaIngredientMapping { PizzaId = 4, Ingredient = PizzaIngredient.Ham },
                new PizzaIngredientMapping { PizzaId = 4, Ingredient = PizzaIngredient.Bacon },

                // Veggie Supreme
                new PizzaIngredientMapping { PizzaId = 5, Ingredient = PizzaIngredient.Mozzarella },
                new PizzaIngredientMapping { PizzaId = 5, Ingredient = PizzaIngredient.Mushrooms },
                new PizzaIngredientMapping { PizzaId = 5, Ingredient = PizzaIngredient.Onions },
                new PizzaIngredientMapping { PizzaId = 5, Ingredient = PizzaIngredient.Bell_Peppers },
                new PizzaIngredientMapping { PizzaId = 5, Ingredient = PizzaIngredient.Olives },
                new PizzaIngredientMapping { PizzaId = 5, Ingredient = PizzaIngredient.Spinach },

                // BBQ Chicken
                new PizzaIngredientMapping { PizzaId = 6, Ingredient = PizzaIngredient.Mozzarella },
                new PizzaIngredientMapping { PizzaId = 6, Ingredient = PizzaIngredient.Chicken },
                new PizzaIngredientMapping { PizzaId = 6, Ingredient = PizzaIngredient.Onions }
            };

            await context.PizzaIngredientMappings.AddRangeAsync(pizzaIngredients);
            await context.SaveChangesAsync();
        }

        if (!await context.Customers.AnyAsync())
        {
            var customers = new List<Customer>
            {
                new Customer
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@pizzaapi.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = "Admin",
                    OrderAmount = 0
                },
                new Customer
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@email.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    Role = "User",
                    OrderAmount = 0
                },
                new Customer
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@email.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    Role = "User",
                    OrderAmount = 0
                },
                new Customer
                {
                    FirstName = "Mike",
                    LastName = "Johnson",
                    Email = "mike.johnson@email.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    Role = "User",
                    OrderAmount = 0
                }
            };

            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
        }
    }
}