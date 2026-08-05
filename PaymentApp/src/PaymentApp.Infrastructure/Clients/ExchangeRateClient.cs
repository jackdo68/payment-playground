using System.Net.Http.Json;

namespace PaymentApp.Infrastructure.Clients;

public record FxRate(string From, string To, decimal Rate, DateOnly Date);

/// <summary>
/// Typed client over a named HttpClient (base URL configured in Program.cs).
/// This is the same shape Topic 10's PaymentProcessorClient uses.
/// </summary>
public class ExchangeRateClient
{
    private readonly HttpClient _client;

    public ExchangeRateClient(IHttpClientFactory factory)
    {
        _client = factory.CreateClient(HttpClientNames.Fx);   // pre-configured named client
    }

    public async Task<FxRate> GetRateAsync(string from, string to)
    {
        // Frankfurter returns:
        // {"amount":1.0,"base":"USD","date":"2026-08-04","rates":{"EUR":0.92}}
        var body = await _client.GetFromJsonAsync<FrankfurterResponse>(
            $"latest?from={from}&to={to}");

        var rate = body!.Rates[to];             // Dictionary lookup
        return new FxRate(from, to, rate, DateOnly.Parse(body.Date));
    }

    // GetFromJsonAsync uses web defaults (case-insensitive) — maps the lowercase JSON.
    private record FrankfurterResponse(
        decimal Amount, string Base, string Date, Dictionary<string, decimal> Rates);
}