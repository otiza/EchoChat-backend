using System.Collections.Concurrent;

namespace Chat.Api.Realtime;

public sealed class InMemoryPresenceTracker
{
    private readonly ConcurrentDictionary<string, int> _counts = new();

    // returns true if the user just became online
    public bool OnConnected(string userId)
    {
        var next = _counts.AddOrUpdate(userId, 1, (_, old) => old + 1);
        return next == 1;
    }
    public IReadOnlyList<string> GetOnlineUserIds()
        => _counts.Keys.ToList();

    // returns true if the user just became offline
    public bool OnDisconnected(string userId)
    {
        while (true)
        {
            if (!_counts.TryGetValue(userId, out var old))
                return false;

            var next = old - 1;

            if (next <= 0)
            {
                if (_counts.TryRemove(userId, out _))
                    return true;
            }
            else
            {
                if (_counts.TryUpdate(userId, next, old))
                    return false;
            }
        }
    }

    public bool IsOnline(string userId) => _counts.ContainsKey(userId);
}