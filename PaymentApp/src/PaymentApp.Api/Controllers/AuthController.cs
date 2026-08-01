using Microsoft.AspNetCore.Mvc;
using PaymentApp.Application.DTOs;
using PaymentApp.Application.Interfaces;

namespace PaymentApp.Api.Controllers;

[ApiController]
[Route("v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
    {
        var user = await _auth.RegisterAsync(request);
        var response = new UserResponse(user.Id, user.Name, user.Email);
        return CreatedAtAction(nameof(Register), new { id = user.Id }, response);
    }
}