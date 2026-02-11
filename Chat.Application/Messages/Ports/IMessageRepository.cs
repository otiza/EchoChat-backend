using Chat.Domain.Entities;

namespace Chat.Application.Messages.Ports;

public interface IMessageRepository
{
    Task<Message> CreateAsync(Message message, CancellationToken ct);

    /// <summary>
    /// Returns newest-first or oldest-first depending on implementation;
    /// we'll define it as newest-first for efficient pagination.
    /// </summary>
    Task<IReadOnlyList<Message>> GetByConversationAsync(
        string conversationId,
        DateTime? before,
        int limit,
        CancellationToken ct);
}