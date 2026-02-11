namespace Chat.Application.Contracts;

public sealed record UserMeResponse(string UserId, string Username, DateTimeOffset CreatedAt);