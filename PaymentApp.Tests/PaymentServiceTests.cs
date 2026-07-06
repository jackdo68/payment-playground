using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PaymentApp.Data;
using PaymentApp.Models;
using PaymentApp.Services.Auth;
using PaymentApp.Services.Payment;
using Xunit;

namespace PaymentApp.Tests;

public class PaymentServiceTests
{
    // Helper: a fresh in-memory DB per test (isolated).
    private static PaymentDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new PaymentDbContext(options);
    }

    // The two services, with their fakes injected BY HAND (no mocking framework).
    private static AuthService NewAuth(PaymentDbContext db) =>
        new(db, new PasswordHasher<User>(), new ConfigurationBuilder().Build());
    private static PaymentService NewPayment(PaymentDbContext db) => new(db);

    // No get-balance endpoint — read the balance straight from the context.
    private static async Task<decimal> BalanceOf(PaymentDbContext db, int id)
        => (await db.Users.FindAsync(id))!.Balance;

    [Fact]  // = test(...) in jest
    public async Task RegisterAsync_CreatesUser_WithStartingBalance()
    {
        var db = NewDb();
        var user = await NewAuth(db).RegisterAsync(
            new RegisterRequest("Alice", "alice@bank.test", "Passw0rd!"));

        Assert.True(user.Id > 0);
        Assert.Equal(1000m, user.Balance);
        Assert.NotEqual("Passw0rd!", user.PasswordHash);   // hashed, never plaintext
    }

    [Fact]
    public async Task TransferAsync_MovesMoney_Exactly()
    {
        var db = NewDb();
        var alice = await NewAuth(db).RegisterAsync(new RegisterRequest("Alice", "a@t.t", "x"));
        var bob = await NewAuth(db).RegisterAsync(new RegisterRequest("Bob", "b@t.t", "x"));

        await NewPayment(db).TransferAsync(alice.Id, bob.Id, 250m);

        Assert.Equal(750m, await BalanceOf(db, alice.Id));
        Assert.Equal(1250m, await BalanceOf(db, bob.Id));
    }

    [Fact]
    public async Task TransferAsync_Throws_WhenInsufficientFunds()
    {
        var db = NewDb();
        var alice = await NewAuth(db).RegisterAsync(new RegisterRequest("Alice", "a@t.t", "x"));
        var bob = await NewAuth(db).RegisterAsync(new RegisterRequest("Bob", "b@t.t", "x"));

        // = expect(...).rejects.toThrow(), but asserting the exception TYPE
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => NewPayment(db).TransferAsync(alice.Id, bob.Id, 5000m));

        Assert.Equal(1000m, await BalanceOf(db, alice.Id));   // nothing moved
    }
}