using ProcesadorCorreosPYS.Domain.Models;

namespace ProcesadorCorreosPYS.Application.Abstractions;

public interface ILogSink
{
    void Publish(ProcessingLogEntry entry);
}
