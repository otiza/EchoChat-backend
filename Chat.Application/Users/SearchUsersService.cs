using Chat.Application.Abstractions.Persistence;
using Chat.Application.Contracts;

namespace Chat.Application.Users;

public sealed class SearchUsersService
{
    private readonly IUserRepository _users;

    public SearchUsersService(IUserRepository users)
    {
        _users = users;
    }

    public async Task<IReadOnlyList<UserSearchResponse>> SearchAsync(string query, int limit = 10, CancellationToken ct = default)
    {
        query = (query ?? string.Empty).Trim();
        if (query.Length < 2)
            return Array.Empty<UserSearchResponse>();

        limit = limit <= 0 ? 10 : Math.Min(limit, 50);

        var users = await _users.SearchByUsernameAsync(query, limit, ct);

        return users
            .Select(u => new UserSearchResponse(u.Id, u.Username))
            .ToList();
    }
}