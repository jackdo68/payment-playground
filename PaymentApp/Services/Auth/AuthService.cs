using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentApp.Data;
using PaymentApp.Models;

namespace PaymentApp.Services.Auth;

public class AuthService(
    PaymentDbContext db,
    IPasswordHasher<User> hasher,
    IConfiguration config
    ) : IAuthService
{
    private readonly PaymentDbContext _db = db;
    private readonly IPasswordHasher<User> _hasher = hasher;
    private readonly IConfiguration _config = config;

    public string CreateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            claims: [
                new Claim("sub", user.Id.ToString()),
                new Claim("name", user.Name),
                new Claim("email", user.Email)
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }



    public async Task<User> RegisterAsync(RegisterRequest request)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Balance = 1000
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null) return null;
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Success ? user : null;
    }
}