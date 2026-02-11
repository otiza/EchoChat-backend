using Chat.Infrastructure.Conversations.Mongo;
using Chat.Infrastructure.Messages.Mongo;
using MongoDB.Driver;

namespace Chat.Infrastructure.Persistence.Chat;

public sealed class ChatMongoIndexes
{
    private readonly IMongoDatabase _database;

    public ChatMongoIndexes(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task EnsureCreatedAsync(CancellationToken ct)
    {
        await EnsureConversationIndexesAsync(ct);
        await EnsureMessageIndexesAsync(ct);
    }

    private async Task EnsureConversationIndexesAsync(CancellationToken ct)
    {
        var conversations =
            _database.GetCollection<ConversationDocument>("conversations");

        var indexes = new List<CreateIndexModel<ConversationDocument>>
        {
            new(
                Builders<ConversationDocument>.IndexKeys.Ascending(x => x.ParticipantUserIds)
            ),
            new(
                Builders<ConversationDocument>.IndexKeys
                    .Ascending(x => x.ParticipantUserIds)
                    .Descending(x => x.LastMessageAt)
            ),
            new(
                Builders<ConversationDocument>.IndexKeys.Ascending(x => x.DirectKey),
                new CreateIndexOptions
                {
                    Name = "ux_conversations_directKey",
                    Unique = true,
                    Sparse = true
                }
            )
        };

        await conversations.Indexes.CreateManyAsync(indexes, ct);
    }

    private async Task EnsureMessageIndexesAsync(CancellationToken ct)
    {
        var messages =
            _database.GetCollection<MessageDocument>("messages");

        var indexes = new List<CreateIndexModel<MessageDocument>>
        {
            // Fast message history paging per conversation
            new(
                Builders<MessageDocument>.IndexKeys
                    .Ascending(x => x.ConversationId)
                    .Descending(x => x.SentAt)
            )
        };

        await messages.Indexes.CreateManyAsync(indexes, ct);
    }
}