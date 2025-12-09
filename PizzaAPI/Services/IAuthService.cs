using PizzaAPI.DTOs;

namespace PizzaAPI.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<CustomerDto?> GetCurrentUserAsync(int userId);
}
