using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentApp.Models;
using PaymentApp.Services.Auth;

namespace PaymentApp.Controllers;

[ApiController]
[Route("v1/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    private readonly IAuthService _auth = auth;

    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
    {
        try
        {

            var user = await _auth.RegisterAsync(request);
            return Ok(new { token = _auth.CreateToken(user) });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { error = "That email is already registered" });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginRequest request)
    {
        var user = await _auth.ValidateCredentialsAsync(request.Email, request.Password);
        if (user is null) return Unauthorized(new { error = "Invalid email or password" });
        return Ok(new { token = _auth.CreateToken(user) });
    }
}