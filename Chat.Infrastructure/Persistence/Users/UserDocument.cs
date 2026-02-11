using Chat.Domain.Users;

namespace Chat.Infrastructure.Persistence.Users;

internal static class UserMapper
{
    public static UserDocument ToDocument(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        PasswordHash = user.PasswordHash,
        CreatedAt = user.CreatedAt
    };

    public static User ToDomain(UserDocument doc) =>
        new(id: doc.Id, username: doc.Username, passwordHash: doc.PasswordHash, createdAt: doc.CreatedAt);
}