namespace Chat.Application.Messages.Contracts;

public sealed record MessageResponse(
    string Id,
    string ConversationId,
    string SenderUserId,
    string Content,
    DateTime SentAt
);