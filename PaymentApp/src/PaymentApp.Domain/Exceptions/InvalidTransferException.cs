namespace PaymentApp.Domain.Exceptions;

public class InvalidTransferException : DomainException
{
    public InvalidTransferException(string reason)
        : base("INVALID_TRANSFER", reason)
    {
    }

    public static InvalidTransferException NegativeAmount(decimal amount)
        => new($"Transfer amount must be positive, got {amount:C}");

    public static InvalidTransferException ZeroAmount()
        => new("Transfer amount cannot be zero");

    public static InvalidTransferException SameUser()
        => new("Cannot transfer to yourself");
}