namespace ProcesadorCorreosPYS.Domain.Models;

public sealed class DailyConsecutiveState
{
    public required string DateKey { get; init; }
    public int LastConsecutive { get; init; }
}
