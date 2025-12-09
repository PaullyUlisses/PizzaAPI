using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PizzaAPI.Data;
using PizzaAPI.DTOs;
using PizzaAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PizzaAPI.Services;

public class AuthService : IAuthService
{
    private readonly PizzaDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(PizzaDbContext context, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == loginDto.Email);

        if (customer == null)
            return null;

        // Verify password with BCrypt
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, customer.PasswordHash))
            return null;

        var token = GenerateJwtToken(customer);
        var expiresAt = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationInHours);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Customer = MapToCustomerDto(customer)
        };
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        // Check if email already exists
        if (await _context.Customers.AnyAsync(c => c.Email == registerDto.Email))
            return null;

        var customer = new Customer
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            Role = "User", // Default role
            OrderAmount = 0
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(customer);
        var expiresAt = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationInHours);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Customer = MapToCustomerDto(customer)
        };
    }

    public async Task<CustomerDto?> GetCurrentUserAsync(int userId)
    {
        var customer = await _context.Customers.FindAsync(userId);
        return customer != null ? MapToCustomerDto(customer) : null;
    }

    private string GenerateJwtToken(Customer customer)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, customer.Email),
            new Claim(ClaimTypes.Role, customer.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpirationInHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static CustomerDto MapToCustomerDto(Customer customer)
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
