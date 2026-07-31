namespace PaymentApp.Domain.ValueObjects;

public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0m, currency);

    public static Money USD(decimal amount) => new(amount, "USD");
    public static Money AUD(decimal amount) => new(amount, "AUD");

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {Currency} to {other.Currency}");

        return this with { Amount = Amount + other.Amount };
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract {other.Currency} from {Currency}");

        return this with { Amount = Amount - other.Amount };
    }

    public override string ToString() => $"{Currency} {Amount:N2}";
}