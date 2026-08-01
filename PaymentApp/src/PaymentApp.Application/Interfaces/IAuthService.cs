using PaymentApp.Application.DTOs;
using PaymentApp.Domain.Entities;

namespace PaymentApp.Application.Interfaces;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterRequest request);
}