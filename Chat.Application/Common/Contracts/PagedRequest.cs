namespace Chat.Application.Common.Contracts;

public sealed record PagedRequest(DateTime? Before, int Limit = 50)
{
    public int LimitSafe => Limit is <= 0 ? 50 : Math.Min(Limit, 200);
}