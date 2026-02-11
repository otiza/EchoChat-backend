
using System.Security.Claims;
using Chat.Application.Auth;
using Microsoft.AspNetCore.Http;

namespace Chat.Infrastructure.Auth;

public sealed class HttpUserContext : IUserContext
{
    private readonly IHttpContextAccessor _http;

    public HttpUserContext(IHttpContextAccessor http)
    {
        _http = http;
    }

    public string UserId =>
        _http.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? _http.HttpContext?.User?.FindFirst("sub")?.Value
        ?? throw new InvalidOperationException("No authenticated user (missing user id claim).");

    public string Username =>
        _http.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
        ?? _http.HttpContext?.User?.FindFirst("unique_name")?.Value
        ?? throw new InvalidOperationException("No authenticated user (missing username claim).");
}