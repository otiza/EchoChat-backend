using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Chat.Domain.Users;

public sealed class User
{
    public string Id { get; private set; } = default!;
    public string Username { get; private set; } = default!;

    // Don’t serialize this in API responses by accident.
    // (MongoDB ignores JsonIgnore; it stores all properties unless you tell it otherwise in Infrastructure.)
    [JsonIgnore]
    public string PasswordHash { get; private set; } = default!;

    public DateTimeOffset CreatedAt { get; private set; }

    private User() { } // for Mongo/serializers

    public User(string id, string username, string passwordHash, DateTimeOffset createdAt)
    {
        Id = NormalizeId(id);
        Username = NormalizeUsername(username);
        PasswordHash = ValidatePasswordHash(passwordHash);
        CreatedAt = ValidateCreatedAt(createdAt);
    }

    // Optional: domain operation if you want to allow username change later
    public void ChangeUsername(string username)
    {
        Username = NormalizeUsername(username);
    }

    // --- Validation / normalization ---

    private static string NormalizeId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id is required.", nameof(id));

        return id.Trim();
        // If you enforce Guid ids in your domain, replace with:
        // return Guid.Parse(id.Trim()).ToString("N");
    }

    private static string NormalizeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));

        var normalized = username.Trim();

        // Minimal sensible constraints (tune later):
        if (normalized.Length is < 3 or > 32)
            throw new ArgumentException("Username must be between 3 and 32 characters.", nameof(username));

        return normalized;
    }

    private static string ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("PasswordHash is required.", nameof(passwordHash));

        return passwordHash.Trim();
    }

    private static DateTimeOffset ValidateCreatedAt(DateTimeOffset createdAt)
    {
        if (createdAt == default)
            throw new ArgumentException("CreatedAt is required.", nameof(createdAt));

        // Avoid future timestamps (allow a little clock skew)
        var now = DateTimeOffset.UtcNow;
        if (createdAt > now.AddMinutes(5))
            throw new ArgumentException("CreatedAt cannot be in the future.", nameof(createdAt));

        return createdAt;
    }

    // Ensures invariants are checked even if a serializer bypasses the public ctor.
    [OnDeserialized]
    internal void OnDeserialized(StreamingContext _)
    {
        Id = NormalizeId(Id);
        Username = NormalizeUsername(Username);
        PasswordHash = ValidatePasswordHash(PasswordHash);
        CreatedAt = ValidateCreatedAt(CreatedAt);
    }
}