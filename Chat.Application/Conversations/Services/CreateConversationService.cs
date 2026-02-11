using Chat.Application.Auth;
using Chat.Application.Common;
using Chat.Application.Conversations.Contracts;
using Chat.Application.Conversations.Ports;
using Chat.Domain.Entities;
using Chat.Application.Abstractions.Persistence;

namespace Chat.Application.Conversations.Services;

public sealed class CreateConversationService
{
    private readonly IUserContext _user;
    private readonly IClock _clock;
    private readonly IConversationRepository _conversations;
    private readonly IUserRepository _users;

    public CreateConversationService(
        IUserContext user,
        IClock clock,
        IConversationRepository conversations,
        IUserRepository users)
    {
        _user = user;
        _clock = clock;
        _conversations = conversations;
        _users = users;
    }

    public async Task<ConversationSummaryResponse> CreateAsync(
        IReadOnlyList<string> participantUserIds,
        CancellationToken ct)
    {
        var participants = participantUserIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Append(_user.UserId)
            .Distinct()
            .ToArray();
        
        // validate all participant ids exist (except current user)
        var otherIds = participants.Where(id => id != _user.UserId).ToArray();
        foreach (var id in otherIds)
        {
            var exists = await _users.ExistsByIdAsync(id, ct);
            if (!exists)
                throw new InvalidOperationException($"User not found: {id}");
        }
        //if there are only 2 participants, check if a conversation between them already exists
        if (participants.Length == 2)
        {
            var a = participants[0];
            var b = participants[1];
            var directKey = string.CompareOrdinal(a, b) < 0 ? $"{a}:{b}" : $"{b}:{a}";

            var conv = new Conversation(
                id: Guid.NewGuid().ToString("N"),
                participantUserIds: participants,
                createdAt: _clock.UtcNow);

            var createdOrExisting = await _conversations.CreateDirectOrGetExistingAsync(conv, directKey, ct);

            return new ConversationSummaryResponse(
                createdOrExisting.Id,
                createdOrExisting.ParticipantUserIds.ToList(),
                createdOrExisting.CreatedAt,
                createdOrExisting.LastMessageAt);
        }
        var conversation = new Conversation(
            id: Guid.NewGuid().ToString("N"),
            participantUserIds: participants,
            createdAt: _clock.UtcNow);

        var created = await _conversations.CreateAsync(conversation, ct);

        return new ConversationSummaryResponse(
            created.Id,
            created.ParticipantUserIds.ToList(),
            created.CreatedAt,
            created.LastMessageAt);
    }
}