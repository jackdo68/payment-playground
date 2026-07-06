using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaymentApp.Data;

namespace PaymentApp.Services.Payment;

public class PaymentService(PaymentDbContext db) : IPaymentService
{
    private readonly PaymentDbContext _db = db;

    public async Task TransferAsync(int payerId, int payeeId, decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");

        var payer = await _db.Users.FirstOrDefaultAsync(a => a.Id == payerId) ?? throw new KeyNotFoundException($"No account for user {payerId}");
        var payee = await _db.Users.FirstOrDefaultAsync(a => a.Id == payeeId) ?? throw new KeyNotFoundException($"No account for user {payeeId}");
        if (payer.Balance < amount) throw new InvalidOperationException("Insufficient funds.");
        payer.Balance -= amount;
        payee.Balance += amount;
        await _db.SaveChangesAsync();
    }
}