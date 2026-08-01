using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaymentApp.Application.DTOs;
using PaymentApp.Application.Interfaces;
using PaymentApp.Domain.Constants;
using PaymentApp.Domain.Entities;
using PaymentApp.Domain.Exceptions;
using PaymentApp.Infrastructure.Data;

namespace PaymentApp.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly PaymentDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public AuthService(PaymentDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<User> RegisterAsync(RegisterRequest request)
    {
        // Check for duplicate email
        var exists = await _db.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
            throw new DuplicateEmailException(request.Email);

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        // Hash password (salted, secure)
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        // Set initial balance
        user.SetInitialBalance(PaymentDefaults.InitialBalance);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return user;
    }
}