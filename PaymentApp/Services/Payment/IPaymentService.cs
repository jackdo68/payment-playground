namespace PaymentApp.Services.Payment;

public interface IPaymentService
{
    Task TransferAsync(int payerId, int payeeUserId, decimal amount);
}