using Chat.Application.Abstractions.Persistence;
using Chat.Application.Contracts;

namespace Chat.Application.Users;

public sealed class GetMeService
{
    private readonly IUserRepository _users;

    public GetMeService(IUserRepository users)
    {
        _users = users;
    }

    public async Task<UserMeResponse> GetMeAsync(string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));

        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null)
            throw new InvalidOperationException("User not found.");

        return new UserMeResponse(user.Id, user.Username, user.CreatedAt);
    }
}