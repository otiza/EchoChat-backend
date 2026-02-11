using Chat.Domain.Entities;

namespace Chat.Application.Conversations.Ports;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(string conversationId, CancellationToken ct);

    /// <summary>Lists conversations where user is a participant (ordered by last activity desc).</summary>
    Task<IReadOnlyList<Conversation>> GetForUserAsync(string userId, int limit, CancellationToken ct);

    /// <summary>Creates a conversation. Should enforce any uniqueness strategy at infra level if needed.</summary>
    Task<Conversation> CreateAsync(Conversation conversation, CancellationToken ct);

    /// <summary>Updates LastMessageAt (or other metadata) when a new message is sent.</summary>
    Task TouchAsync(string conversationId, DateTime lastMessageAt, CancellationToken ct);

    /// <summary>True if user is participant.</summary>
    Task<bool> IsParticipantAsync(string conversationId, string userId, CancellationToken ct);
    
    /// Return Conv (DM) between two Users
    Task<Conversation?> FindDirectAsync(string userId1, string userId2, CancellationToken ct);
    Task<Conversation?> GetByDirectKeyAsync(string directKey, CancellationToken ct);
    Task<Conversation> CreateDirectOrGetExistingAsync(Conversation conversation, string directKey, CancellationToken ct);
    
    Task<IReadOnlyList<string>> GetIdsForUserAsync(string userId, int limit, CancellationToken ct);
    Task<IReadOnlyList<string>> GetParticipantIdsAsync(string conversationId, CancellationToken ct);
    Task<IReadOnlyList<string>> GetContactUserIdsAsync(string userId, CancellationToken ct);
    Task<IReadOnlyList<string>> GetParticipantUserIdsAsync(string conversationId, CancellationToken ct);
    Task TouchLastMessageAsync(string conversationId, DateTime lastMessageAt, string? lastMessagePreview, CancellationToken ct);
}
