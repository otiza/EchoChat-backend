using Chat.Application.Auth;
using Chat.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Chat.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly RegisterService _register;
    private readonly LoginService _login;

    public AuthController(RegisterService register, LoginService login)
    {
        _register = register;
        _login = login;
    }
    
    [HttpPost("register")]
    public async Task<AuthResponse> Register([FromBody] RegisterRequest request, CancellationToken ct)
        => await _register.RegisterAsync(request, ct);

    [HttpPost("login")]
    public async Task<AuthResponse> Login([FromBody] LoginRequest request, CancellationToken ct)
        => await _login.LoginAsync(request, ct);
    
}