namespace PaymentApp.Domain.Exceptions;

public class InsufficientBalanceException : DomainException
{
    public decimal CurrentBalance { get; }
    public decimal RequestedAmount { get; }

    public InsufficientBalanceException(decimal currentBalance, decimal requestedAmount)
        : base(
            "INSUFFICIENT_BALANCE",
            $"Cannot withdraw {requestedAmount:C} with balance of {currentBalance:C}")
    {
        CurrentBalance = currentBalance;
        RequestedAmount = requestedAmount;
    }
}