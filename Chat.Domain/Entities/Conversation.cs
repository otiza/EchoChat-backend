using System;
using System.Collections.Generic;
using System.Linq;

namespace Chat.Domain.Entities;

public sealed class Conversation
{
    public string Id { get; }
    public IReadOnlyCollection<string> ParticipantUserIds { get; }
    public DateTime CreatedAt { get; }
    public DateTime? LastMessageAt { get; private set; }

    public Conversation(
        string id,
        IEnumerable<string> participantUserIds,
        DateTime createdAt,
        DateTime? lastMessageAt = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Conversation id is required.", nameof(id));

        if (participantUserIds is null)
            throw new ArgumentNullException(nameof(participantUserIds));

        var participants = participantUserIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToArray();

        if (participants.Length < 2)
            throw new ArgumentException("A conversation must have at least 2 participants.", nameof(participantUserIds));

        Id = id;
        ParticipantUserIds = participants;
        CreatedAt = createdAt;
        LastMessageAt = lastMessageAt;
    }

    public void Touch(DateTime at)
    {
        // Keep the max timestamp (defensive ordering)
        if (LastMessageAt is null || at > LastMessageAt.Value)
            LastMessageAt = at;
    }
}