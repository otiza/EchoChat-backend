using Chat.Application.Abstractions.Authentication;
using Chat.Application.Abstractions.Persistence;
using Chat.Application.Contracts;
using Chat.Domain.Users;

namespace Chat.Application.Auth;

public sealed class RegisterService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;

    public RegisterService(IUserRepository users, IPasswordHasher hasher, IJwtTokenGenerator jwt)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var username = request.Username?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(request.Username));

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters.", nameof(request.Password));

        if (await _users.ExistsByUsernameAsync(username, ct))
            throw new InvalidOperationException("Username already exists.");

        var passwordHash = _hasher.Hash(request.Password);

        // keep id generation in Application (simple) for now
        var user = new User(
            id: Guid.NewGuid().ToString("N"),
            username: username,
            passwordHash: passwordHash,
            createdAt: DateTimeOffset.UtcNow);

        await _users.CreateAsync(user, ct);

        var token = _jwt.GenerateToken(user);
        return new AuthResponse(token, user.Id, user.Username);
    }
}