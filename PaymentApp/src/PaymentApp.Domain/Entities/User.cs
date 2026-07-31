using PaymentApp.Domain.Events;
using PaymentApp.Domain.Exceptions;

namespace PaymentApp.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public decimal Balance { get; private set; }
    public string? DocumentPath { get; set; }
    /// <summary>
    /// Withdraws money from this user's balance.
    /// </summary>
    /// <exception cref="InsufficientBalanceException">
    /// Thrown when balance is insufficient.
    /// </exception>
    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw InvalidTransferException.NegativeAmount(amount);

        if (Balance < amount)
            throw new InsufficientBalanceException(Balance, amount);

        var oldBalance = Balance;
        Balance -= amount;

        AddDomainEvent(new UserBalanceChanged(Id, oldBalance, Balance));
    }

    /// <summary>
    /// Deposits money into this user's balance.
    /// </summary>
    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw InvalidTransferException.NegativeAmount(amount);

        var oldBalance = Balance;
        Balance += amount;

        AddDomainEvent(new UserBalanceChanged(Id, oldBalance, Balance));
    }

    /// <summary>
    /// Sets the initial balance (for account creation).
    /// </summary>
    public void SetInitialBalance(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Initial balance cannot be negative", nameof(amount));

        Balance = amount;
    }
}