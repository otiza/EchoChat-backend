using Chat.Application.Messages.Ports;
using Chat.Domain.Entities;
using Chat.Infrastructure.Messages.Mongo;
using MongoDB.Driver;

namespace Chat.Infrastructure.Messages;

public sealed class MessageMongoRepository : IMessageRepository
{
    private readonly IMongoCollection<MessageDocument> _collection;

    public MessageMongoRepository(IMongoDatabase db)
    {
        _collection = db.GetCollection<MessageDocument>("messages");
    }

    public async Task<Message> CreateAsync(Message message, CancellationToken ct)
    {
        var doc = ToDoc(message);
        await _collection.InsertOneAsync(doc, cancellationToken: ct);
        return ToDomain(doc);
    }

    public async Task<IReadOnlyList<Message>> GetByConversationAsync(
        string conversationId,
        DateTime? before,
        int limit,
        CancellationToken ct)
    {
        var filter = Builders<MessageDocument>.Filter.Eq(x => x.ConversationId, conversationId);

        if (before is not null)
            filter &= Builders<MessageDocument>.Filter.Lt(x => x.SentAt, before.Value);

        // NEWEST-first (matches your Application comment)
        var docs = await _collection
            .Find(filter)
            .SortByDescending(x => x.SentAt)
            .Limit(limit)
            .ToListAsync(ct);

        return docs.Select(ToDomain).ToList();
    }

    private static Message ToDomain(MessageDocument doc)
        => new(
            id: doc.Id,
            conversationId: doc.ConversationId,
            senderUserId: doc.SenderUserId,
            content: doc.Content,
            sentAt: doc.SentAt);

    private static MessageDocument ToDoc(Message m)
        => new()
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            SenderUserId = m.SenderUserId,
            Content = m.Content,
            SentAt = m.SentAt
        };
}