namespace PaymentApp.Domain.Constants;

public static class PaymentDefaults
{
    public const decimal InitialBalance = 1000m;
    public const string DefaultCurrency = "USD";

    public static class TestUsers
    {
        public const string AliceEmail = "alice@bank.test";
        public const string BobEmail = "bob@bank.test";
        public const string CaraEmail = "cara@bank.test";
        public const string DefaultPassword = "Passw0rd!";
    }
}