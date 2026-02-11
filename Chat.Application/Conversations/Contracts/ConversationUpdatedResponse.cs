namespace Chat.Application.Conversations.Contracts;

public sealed record ConversationUpdatedResponse(
    string ConversationId,
    string LastMessagePreview,
    DateTime LastMessageAt,
    string LastMessageSenderUserId
);  