namespace ProcesadorCorreosPYS.Domain.Models;

public sealed class EmailQuery
{
    public required DateTimeOffset Start { get; init; }
    public required DateTimeOffset End { get; init; }
    public string? LastProcessedMessageId { get; init; }
    public int MaxItems { get; init; } = 500;
}
