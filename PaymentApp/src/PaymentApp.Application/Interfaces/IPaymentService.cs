namespace PaymentApp.Application.Interfaces;

public interface IPaymentService
{
    Task TransferAsync(int payerUserId, int payeeUserId, decimal amount);
}