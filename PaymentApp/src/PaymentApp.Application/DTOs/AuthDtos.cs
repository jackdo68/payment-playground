namespace PaymentApp.Application.DTOs;

public record RegisterRequest(string Name, string Email, string Password);

public record UserResponse(int Id, string Name, string Email);