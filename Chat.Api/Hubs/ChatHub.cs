using System.Security.Claims;
using Chat.Application.Conversations.Ports;
using Chat.Application.Messages.Contracts;
using Chat.Application.Messages.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Api.Hubs;

[Authorize]
public sealed class ChatHub : Hub
{
    private readonly IConversationRepository _conversations;
    private readonly SendMessageService _sendMessage;
    private readonly Chat.Api.Realtime.InMemoryPresenceTracker _presence;

    public ChatHub(
        IConversationRepository conversations,
        SendMessageService sendMessage,
        Chat.Api.Realtime.InMemoryPresenceTracker presence)
    {
        _conversations = conversations;
        _sendMessage = sendMessage;
        _presence = presence;
    }

    private string GetUserIdOrThrow()
    {
        var userId =
            Context.User?.FindFirstValue("sub") ??
            Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            throw new HubException("Unauthorized: missing user id claim (sub).");

        return userId;
    }

    public async Task JoinConversation(string conversationId)
    {
        var userId = GetUserIdOrThrow();
        var ct = Context.ConnectionAborted;

        var isParticipant = await _conversations.IsParticipantAsync(conversationId, userId, ct);
        if (!isParticipant)
            throw new HubException("Forbidden: not a participant in this conversation.");

        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId, ct);
    }

    public Task LeaveConversation(string conversationId)
    {
        var ct = Context.ConnectionAborted;
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId, ct);
    }

    public async Task<MessageResponse> SendMessage(string conversationId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new HubException("Message content is empty.");

        if (content.Length > 2000)
            throw new HubException("Message is too long (max 2000 characters).");

        var ct = Context.ConnectionAborted;

        var msg = await _sendMessage.SendAsync(conversationId, content.Trim(), ct);

        var participants = await _conversations.GetParticipantUserIdsAsync(conversationId, ct);

        // don't echo to sender (sender already has msg as return value)
        var others = participants.Where(id => id != msg.SenderUserId).ToList();

        await Clients.Users(others).SendAsync("MessageReceived", msg, ct);

        return msg;
    }
    
    public async Task<IReadOnlyList<string>> GetOnlineContacts()
    {
        var userId = GetUserIdOrThrow();
        var ct = Context.ConnectionAborted;

        var contacts = await _conversations.GetContactUserIdsAsync(userId, ct);
        var online = _presence.GetOnlineUserIds();

        // return only online contacts
        return contacts.Intersect(online).ToList();
    }
    
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserIdOrThrow();
        var ct = Context.ConnectionAborted;

        // Ensure user-group exists (you already do this in Step 7)
        await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId), ct);

        // Auto-join conversations (you already do this)
        const int maxAutoJoin = 200;
        var conversationIds = await _conversations.GetIdsForUserAsync(userId, maxAutoJoin, ct);
        foreach (var id in conversationIds)
            await Groups.AddToGroupAsync(Context.ConnectionId, id, ct);

        // Presence: only broadcast if user just became online
        if (_presence.OnConnected(userId))
            await BroadcastPresenceAsync(userId, isOnline: true, ct);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserIdOrThrow();

        // Important: connection is closing -> ConnectionAborted is already canceled
        var becameOffline = _presence.OnDisconnected(userId);

        if (becameOffline)
        {
            try
            {
                await BroadcastPresenceAsync(userId, isOnline: false, CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(ex, "Failed to broadcast offline presence for {UserId}", userId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
    
    private static string UserGroup(string userId) => $"user:{userId}";

    private async Task BroadcastPresenceAsync(string userId, bool isOnline, CancellationToken ct)
    {
        // if ct is canceled (disconnect), still try best-effort
        var safeCt = ct.CanBeCanceled && ct.IsCancellationRequested ? CancellationToken.None : ct;

        var contacts = await _conversations.GetContactUserIdsAsync(userId, safeCt);

        foreach (var contactId in contacts)
        {
            await Clients.User(contactId).SendAsync(
                "UserPresenceChanged",
                new { userId, isOnline },
                safeCt);
        }
    }
}