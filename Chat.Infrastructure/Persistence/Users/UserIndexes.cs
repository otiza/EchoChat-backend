using MongoDB.Driver;

namespace Chat.Infrastructure.Persistence.Users;

public static class UserIndexes
{
    public static async Task EnsureCreatedAsync(IMongoDatabase db, CancellationToken ct = default)
    {
        var collection = db.GetCollection<UserDocument>("users");

        var usernameIndex = new CreateIndexModel<UserDocument>(
            Builders<UserDocument>.IndexKeys.Ascending(x => x.Username),
            new CreateIndexOptions
            {
                Unique = true,
                Name = "ux_users_username"
            });

        await collection.Indexes.CreateOneAsync(usernameIndex, cancellationToken: ct);
    }
}