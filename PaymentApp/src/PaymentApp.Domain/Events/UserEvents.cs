namespace PaymentApp.Domain.Events;

public record UserRegistered(int UserId, string Email) : DomainEvent;

public record UserBalanceChanged(int UserId, decimal OldBalance, decimal NewBalance) : DomainEvent;