namespace Chat.Infrastructure.Persistence.Mongo;

public sealed class MongoOptions
{
    public const string SectionName = "Mongo";

    public required string ConnectionString { get; init; }
    public required string Database { get; init; }
}