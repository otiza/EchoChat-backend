namespace Chat.Application.Auth;

/// <summary>Abstraction to access the current authenticated user.</summary>
public interface IUserContext
{
    string UserId { get; }
    string Username { get; }
}