namespace PaymentApp.Domain.Exceptions;

/// <summary>
/// Base class for all domain exceptions.
/// Domain exceptions represent business rule violations.
/// </summary>
public abstract class DomainException : Exception
{
    public string Code { get; }

    protected DomainException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    protected DomainException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}