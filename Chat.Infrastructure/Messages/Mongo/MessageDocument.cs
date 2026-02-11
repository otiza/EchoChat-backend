using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Chat.Infrastructure.Messages.Mongo;

public sealed class MessageDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = default!;

    [BsonRepresentation(BsonType.String)]
    public string ConversationId { get; set; } = default!;

    [BsonRepresentation(BsonType.String)]
    public string SenderUserId { get; set; } = default!;

    public string Content { get; set; } = default!;

    public DateTime SentAt { get; set; }
}