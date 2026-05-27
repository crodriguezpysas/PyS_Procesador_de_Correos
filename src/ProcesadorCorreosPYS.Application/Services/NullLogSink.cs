using ProcesadorCorreosPYS.Application.Abstractions;
using ProcesadorCorreosPYS.Domain.Models;

namespace ProcesadorCorreosPYS.Application.Services;

public sealed class NullLogSink : ILogSink
{
    public void Publish(ProcessingLogEntry entry)
    {
    }
}
