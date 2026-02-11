using System;

namespace Chat.Domain.Entities;

public sealed class Message
{
    public string Id { get; }
    public string ConversationId { get; }
    public string SenderUserId { get; }
    public string Content { get; }
    public DateTime SentAt { get; }

    public Message(
        string id,
        string conversationId,
        string senderUserId,
        string content,
        DateTime sentAt)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Message id is required.", nameof(id));

        if (string.IsNullOrWhiteSpace(conversationId))
            throw new ArgumentException("ConversationId is required.", nameof(conversationId));

        if (string.IsNullOrWhiteSpace(senderUserId))
            throw new ArgumentException("SenderUserId is required.", nameof(senderUserId));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required.", nameof(content));

        Id = id;
        ConversationId = conversationId;
        SenderUserId = senderUserId;
        Content = content.Trim();
        SentAt = sentAt;
    }
}