namespace PaymentApp.Infrastructure.Clients;

/// <summary>
/// Single source of truth for named-HttpClient keys. Registration and consumer
/// both reference the constant, so they can never drift.
/// </summary>
public static class HttpClientNames
{
    public const string Fx = "fx";
    public const string PaymentProcessor = "processor";   // Topic 10 reuses this
}