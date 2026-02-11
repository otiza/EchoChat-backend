using Chat.Application.Abstractions.Persistence;
using Chat.Domain.Users;
using MongoDB.Driver;

namespace Chat.Infrastructure.Persistence.Users;

public sealed class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<UserDocument> _users;

    public MongoUserRepository(IMongoDatabase db)
    {
        _users = db.GetCollection<UserDocument>("users");
    }

    public async Task<User?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var doc = await _users.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        return doc is null ? null : UserMapper.ToDomain(doc);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        var doc = await _users.Find(x => x.Username == username).FirstOrDefaultAsync(ct);
        return doc is null ? null : UserMapper.ToDomain(doc);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
    {
        var count = await _users.CountDocumentsAsync(x => x.Username == username, cancellationToken: ct);
        return count > 0;
    }

    public Task CreateAsync(User user, CancellationToken ct = default)
        => _users.InsertOneAsync(UserMapper.ToDocument(user), cancellationToken: ct);

    public async Task<IReadOnlyList<User>> SearchByUsernameAsync(string query, int limit, CancellationToken ct = default)
    {
        query ??= string.Empty;
        limit = limit <= 0 ? 10 : Math.Min(limit, 50);

        // simple "contains" search (we can upgrade later with case-insensitive index / collation)
        var docs = await _users
            .Find(x => x.Username.Contains(query))
            .Limit(limit)
            .ToListAsync(ct);

        return docs.Select(UserMapper.ToDomain).ToList();
    }
    
    public async Task<bool> ExistsByIdAsync(string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var count = await _users.CountDocumentsAsync(
            x => x.Id == userId,
            cancellationToken: ct);

        return count > 0;
    }
}