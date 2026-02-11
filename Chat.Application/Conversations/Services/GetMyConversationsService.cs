using Chat.Application.Auth;
using Chat.Application.Conversations.Contracts;
using Chat.Application.Conversations.Ports;

namespace Chat.Application.Conversations.Services;

public sealed class GetMyConversationsService
{
    private readonly IUserContext _user;
    private readonly IConversationRepository _conversations;

    public GetMyConversationsService(IUserContext user, IConversationRepository conversations)
    {
        _user = user;
        _conversations = conversations;
    }

    public async Task<IReadOnlyList<ConversationSummaryResponse>> GetAsync(int limit, CancellationToken ct)
    {
        var lim = limit <= 0 ? 50 : Math.Min(limit, 200);

        var list = await _conversations.GetForUserAsync(_user.UserId, lim, ct);

        return list.Select(c => new ConversationSummaryResponse(
                c.Id,
                c.ParticipantUserIds.ToList(),
                c.CreatedAt,
                c.LastMessageAt))
            .ToList();
    }
}