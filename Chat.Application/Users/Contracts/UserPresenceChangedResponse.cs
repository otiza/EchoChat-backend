namespace Chat.Application.Users.Contracts;

public sealed record UserPresenceChangedResponse(
    string UserId,
    bool IsOnline,
    DateTime At
);