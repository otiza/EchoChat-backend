using Chat.Application.Conversations.Ports;
using Chat.Domain.Entities;
using Chat.Infrastructure.Conversations.Mongo;
using MongoDB.Driver;

using MongoDB.Bson;
namespace Chat.Infrastructure.Conversations;

public sealed class ConversationMongoRepository : IConversationRepository
{
    private readonly IMongoCollection<ConversationDocument> _collection;

    public ConversationMongoRepository(IMongoDatabase db)
    {
        _collection = db.GetCollection<ConversationDocument>("conversations");
    }

    public async Task<Conversation?> GetByIdAsync(string conversationId, CancellationToken ct)
    {
        var doc = await _collection
            .Find(x => x.Id == conversationId)
            .FirstOrDefaultAsync(ct);

        return doc is null ? null : ToDomain(doc);
    }

    public async Task<IReadOnlyList<Conversation>> GetForUserAsync(string userId, int limit, CancellationToken ct)
    {
        var docs = await _collection
            .Find(x => x.ParticipantUserIds.Contains(userId))
            .Sort(
                Builders<ConversationDocument>.Sort
                    .Descending(x => x.UpdatedAt)
            )
            .Limit(limit)
            .ToListAsync(ct);

        return docs.Select(ToDomain).ToList();
    }
    
    public async Task<Conversation?> FindDirectAsync(string userId1, string userId2, CancellationToken ct)
    {
        var filter = Builders<ConversationDocument>.Filter.Size(x => x.ParticipantUserIds, 2) &
                     Builders<ConversationDocument>.Filter.All(x => x.ParticipantUserIds, new[] { userId1, userId2 });

        var convDoc = await _collection.Find(filter).FirstOrDefaultAsync(ct);
        return convDoc is null ? null : ToDomain(convDoc);
    }
    
    public async Task<Conversation> CreateAsync(Conversation conversation, CancellationToken ct)
    {
        var doc = ToDoc(conversation);
        await _collection.InsertOneAsync(doc, cancellationToken: ct);
        return ToDomain(doc);
    }

    public async Task TouchAsync(string conversationId, DateTime lastMessageAt, CancellationToken ct)
    {
        var update = Builders<ConversationDocument>.Update
            .Set(x => x.LastMessageAt, lastMessageAt);

        await _collection.UpdateOneAsync(
            x => x.Id == conversationId,
            update,
            cancellationToken: ct);
    }

    public async Task<bool> IsParticipantAsync(string conversationId, string userId, CancellationToken ct)
    {
        // super cheap existence check
        var count = await _collection.CountDocumentsAsync(
            x => x.Id == conversationId && x.ParticipantUserIds.Contains(userId),
            cancellationToken: ct);

        return count > 0;
    }
    
    private static string ComputeDirectKey(string userId1, string userId2)
    {
        return string.CompareOrdinal(userId1, userId2) < 0
            ? $"{userId1}:{userId2}"
            : $"{userId2}:{userId1}";
    }
    
    public async Task<Conversation> CreateDirectOrGetExistingAsync(
        Conversation conversation,
        string directKey,
        CancellationToken ct)
    {
        var doc = ToDoc(conversation);
        doc.DirectKey = directKey;

        try
        {
            await _collection.InsertOneAsync(doc, cancellationToken: ct);
            return ToDomain(doc);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            // Someone else created it at the same time -> fetch existing
            var existing = await _collection
                .Find(x => x.DirectKey == directKey)
                .FirstOrDefaultAsync(ct);

            if (existing is null)
                throw; // very unlikely, but don’t hide it

            return ToDomain(existing);
        }
    }
    
    public async Task<Conversation?> GetByDirectKeyAsync(string directKey, CancellationToken ct)
    {
        var doc = await _collection
            .Find(x => x.DirectKey == directKey)
            .FirstOrDefaultAsync(ct);

        return doc is null ? null : ToDomain(doc);
    }
    
    public async Task<IReadOnlyList<string>> GetIdsForUserAsync(string userId, int limit, CancellationToken ct)
    {
        limit = Math.Clamp(limit, 1, 500); // safety cap

        var ids = await _collection
            .Find(x => x.ParticipantUserIds.Contains(userId))
            .Project(x => x.Id)
            .Limit(limit)
            .ToListAsync(ct);

        return ids;
    }
    
    public async Task<IReadOnlyList<string>> GetParticipantIdsAsync(string conversationId, CancellationToken ct)
    {
        var participants = await _collection
            .Find(x => x.Id == conversationId)
            .Project(x => x.ParticipantUserIds)
            .FirstOrDefaultAsync(ct);

        return participants ?? new List<string>();
    }
    
    public async Task<IReadOnlyList<string>> GetContactUserIdsAsync(string userId, CancellationToken ct)
    {
        var docs = await _collection.Aggregate()
            .Match(Builders<ConversationDocument>.Filter.AnyEq(x => x.ParticipantUserIds, userId))
            .Unwind("ParticipantUserIds")
            .Match(new BsonDocument("ParticipantUserIds", new BsonDocument("$ne", userId)))
            .Group(new BsonDocument { { "_id", "$ParticipantUserIds" } })
            .ToListAsync(ct);

        return docs.Select(d => d["_id"].AsString).ToList();
    }
    
    public async Task<IReadOnlyList<string>> GetParticipantUserIdsAsync(string conversationId, CancellationToken ct)
    {
        var doc = await _collection
            .Find(x => x.Id == conversationId)
            .Project(x => new { x.ParticipantUserIds })
            .FirstOrDefaultAsync(ct);

        if (doc is null)
            throw new InvalidOperationException("Conversation not found.");

        return doc.ParticipantUserIds;
    }


    public Task TouchLastMessageAsync(string conversationId, DateTime lastMessageAt, string? lastMessagePreview, CancellationToken ct)
    {
        var update = Builders<ConversationDocument>.Update
            .Set(x => x.LastMessageAt, lastMessageAt)
            .Set(x => x.LastMessagePreview, lastMessagePreview)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        return _collection.UpdateOneAsync(
            x => x.Id == conversationId,
            update,
            cancellationToken: ct);
    }
    private static Conversation ToDomain(ConversationDocument doc)
        => new(
            id: doc.Id,
            participantUserIds: doc.ParticipantUserIds,
            createdAt: doc.CreatedAt,
            lastMessageAt: doc.LastMessageAt);

    private static ConversationDocument ToDoc(Conversation c)
        => new()
        {
            Id = c.Id,
            ParticipantUserIds = c.ParticipantUserIds.ToList(),
            CreatedAt = c.CreatedAt,
            LastMessageAt = c.LastMessageAt,
            DirectKey = null // default for group chats
        };
    
}