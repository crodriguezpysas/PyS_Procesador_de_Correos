using ProcesadorCorreosPYS.Application.Abstractions;

namespace ProcesadorCorreosPYS.Application.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
