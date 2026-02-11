namespace Chat.Application.Contracts;

public sealed record AuthResponse(string Token, string UserId, string Username);