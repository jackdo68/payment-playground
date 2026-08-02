using Microsoft.EntityFrameworkCore;
using PaymentApp.Application.Interfaces;
using PaymentApp.Domain.Exceptions;
using PaymentApp.Infrastructure.Data;

namespace PaymentApp.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly PaymentDbContext _db;

    // Static semaphore: one gate for all instances (service is scoped)
    // SemaphoreSlim(1, 1) = mutex — only one transfer at a time
    private static readonly SemaphoreSlim _transferGate = new(1, 1);

    public PaymentService(PaymentDbContext db)
    {
        _db = db;
    }

    public async Task TransferAsync(int payerUserId, int payeeUserId, decimal amount)
    {
        // Validation before acquiring the lock
        if (amount <= 0)
            throw InvalidTransferException.NegativeAmount(amount);

        if (payerUserId == payeeUserId)
            throw InvalidTransferException.SameUser();

        // Acquire the semaphore — only one transfer executes at a time
        await _transferGate.WaitAsync();
        try
        {
            // Now safe: no other transfer can read-modify-write concurrently
            var payer = await _db.Users.FirstOrDefaultAsync(u => u.Id == payerUserId)
                ?? throw new UserNotFoundException(payerUserId);

            var payee = await _db.Users.FirstOrDefaultAsync(u => u.Id == payeeUserId)
                ?? throw new UserNotFoundException(payeeUserId);

            // Domain logic handles validation and events
            payer.Withdraw(amount);
            payee.Deposit(amount);

            // One commit, both changes
            await _db.SaveChangesAsync();
        }
        finally
        {
            // ALWAYS release in finally — even if an exception is thrown
            _transferGate.Release();
        }
    }
}