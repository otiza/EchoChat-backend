using Chat.Domain.Users;

namespace Chat.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);

    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);

    Task CreateAsync(User user, CancellationToken ct = default);

    // Optional but useful for chat prerequisites
    Task<IReadOnlyList<User>> SearchByUsernameAsync(string query, int limit, CancellationToken ct = default);
    Task<bool> ExistsByIdAsync(string userId, CancellationToken ct);
}