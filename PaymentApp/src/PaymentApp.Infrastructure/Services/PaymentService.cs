using Microsoft.EntityFrameworkCore;
using PaymentApp.Application.Interfaces;
using PaymentApp.Domain.Exceptions;
using PaymentApp.Infrastructure.Data;

namespace PaymentApp.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly PaymentDbContext _db;

    public PaymentService(PaymentDbContext db)
    {
        _db = db;
    }

    public async Task TransferAsync(int payerUserId, int payeeUserId, decimal amount)
    {
        if (amount <= 0)
            throw InvalidTransferException.NegativeAmount(amount);

        if (payerUserId == payeeUserId)
            throw InvalidTransferException.SameUser();

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
}