namespace Chat.Application.Conversations.Contracts;

public sealed record CreateConversationRequest(IReadOnlyList<string> ParticipantUserIds);