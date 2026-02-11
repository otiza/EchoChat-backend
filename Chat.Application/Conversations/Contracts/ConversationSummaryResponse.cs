namespace Chat.Application.Conversations.Contracts;

public sealed record ConversationSummaryResponse(
    string Id,
    IReadOnlyList<string> ParticipantUserIds,
    DateTime CreatedAt,
    DateTime? LastMessageAt
);