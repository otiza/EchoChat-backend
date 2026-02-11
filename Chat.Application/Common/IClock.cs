namespace Chat.Application.Common;

public interface IClock
{
    DateTime UtcNow { get; }
}