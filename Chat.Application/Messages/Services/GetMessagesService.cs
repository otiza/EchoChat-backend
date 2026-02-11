using Chat.Application.Auth;
using Chat.Application.Common.Contracts;
using Chat.Application.Conversations.Ports;
using Chat.Application.Messages.Contracts;
using Chat.Application.Messages.Ports;

namespace Chat.Application.Messages.Services;

public sealed class GetMessagesService
{
    private readonly IUserContext _user;
    private readonly IConversationRepository _conversations;
    private readonly IMessageRepository _messages;

    public GetMessagesService(
        IUserContext user,
        IConversationRepository conversations,
        IMessageRepository messages)
    {
        _user = user;
        _conversations = conversations;
        _messages = messages;
    }

    public async Task<IReadOnlyList<MessageResponse>> GetAsync(
        string conversationId,
        PagedRequest page,
        CancellationToken ct)
    {
        var isParticipant = await _conversations.IsParticipantAsync(conversationId, _user.UserId, ct);
        if (!isParticipant)
            throw new InvalidOperationException("You are not a participant in this conversation.");

        var list = await _messages.GetByConversationAsync(
            conversationId,
            before: page.Before,
            limit: page.LimitSafe,
            ct);

        // We define repository returns newest-first. If you prefer oldest-first for UI, reverse here.
        return list.Select(m => new MessageResponse(
                m.Id,
                m.ConversationId,
                m.SenderUserId,
                m.Content,
                m.SentAt))
            .ToList();
    }
}