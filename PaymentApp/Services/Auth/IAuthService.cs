
using PaymentApp.Models;

namespace PaymentApp.Services.Auth;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterRequest request);
    Task<User?> ValidateCredentialsAsync(string email, string password);
    string CreateToken(User user);
}