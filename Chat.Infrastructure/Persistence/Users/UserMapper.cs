using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Chat.Infrastructure.Persistence.Users;

internal sealed class UserDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = default!;

    [BsonElement("username")]
    public string Username { get; set; } = default!;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = default!;

    [BsonElement("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }
}