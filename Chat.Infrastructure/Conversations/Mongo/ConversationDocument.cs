using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Chat.Infrastructure.Conversations.Mongo;

public sealed class ConversationDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = default!;

    public List<string> ParticipantUserIds { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public DateTime? LastMessageAt { get; set; }
    
    public string? DirectKey { get; set; }

    public string? LastMessagePreview { get; set; }
    public DateTime UpdatedAt { get; set; }
}