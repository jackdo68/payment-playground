namespace PaymentApp.Models;

public record RegisterRequest(string Name, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record TransferRequest(int PayerId, int PayeeId, decimal Amount);

public record UserResponse(int Id, string Name, string Email);