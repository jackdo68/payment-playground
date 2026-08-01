namespace PaymentApp.Application.DTOs;

public record TransferRequest(int PayerUserId, int PayeeUserId, decimal Amount);

public record TransferResponse(string Status, decimal PayerBalance, decimal PayeeBalance);