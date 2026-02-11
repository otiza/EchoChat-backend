using Chat.Application.Auth;
using Chat.Application.Common;
using Chat.Application.Conversations.Ports;
using Chat.Application.Messages.Contracts;
using Chat.Application.Messages.Ports;
using Chat.Domain.Entities;

namespace Chat.Application.Messages.Services;

public sealed class SendMessageService
{
    private readonly IUserContext _user;
    private readonly IClock _clock;
    private readonly IConversationRepository _conversations;
    private readonly IMessageRepository _messages;

    public SendMessageService(
        IUserContext user,
        IClock clock,
        IConversationRepository conversations,
        IMessageRepository messages)
    {
        _user = user;
        _clock = clock;
        _conversations = conversations;
        _messages = messages;
    }

    public async Task<MessageResponse> SendAsync(string conversationId, string content, CancellationToken ct)
    {
        // AuthZ: must be participant
        var isParticipant = await _conversations.IsParticipantAsync(conversationId, _user.UserId, ct);
        if (!isParticipant)
            throw new InvalidOperationException("You are not a participant in this conversation.");

        var message = new Message(
            id: Guid.NewGuid().ToString("N"),
            conversationId: conversationId,
            senderUserId: _user.UserId,
            content: content,
            sentAt: _clock.UtcNow);

        var created = await _messages.CreateAsync(message, ct);

// preview for list UI (optional)
        var preview = created.Content.Length <= 120
            ? created.Content
            : created.Content[..120];

        await _conversations.TouchLastMessageAsync(
            conversationId,
            created.SentAt,
            preview,
            ct);

        return new MessageResponse(
            created.Id,
            created.ConversationId,
            created.SenderUserId,
            created.Content,
            created.SentAt);
    }
}