using Chat.Application.Abstractions.Authentication;
using Chat.Application.Abstractions.Persistence;
using Chat.Application.Contracts;

namespace Chat.Application.Auth;

public sealed class LoginService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;

    public LoginService(IUserRepository users, IPasswordHasher hasher, IJwtTokenGenerator jwt)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var username = request.Username?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(request.Username));

        var user = await _users.GetByUsernameAsync(username, ct);
        if (user is null)
            throw new InvalidOperationException("Invalid username or password.");

        if (!_hasher.Verify(request.Password ?? string.Empty, user.PasswordHash))
            throw new InvalidOperationException("Invalid username or password.");

        var token = _jwt.GenerateToken(user);
        return new AuthResponse(token, user.Id, user.Username);
    }
}