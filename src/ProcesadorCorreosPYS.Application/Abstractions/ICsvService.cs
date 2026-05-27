using ProcesadorCorreosPYS.Domain.Models;

namespace ProcesadorCorreosPYS.Application.Abstractions;

public interface ICsvService
{
    ValueTask ExportIncrementalAsync(string csvPath, IReadOnlyCollection<ProcessedEmailRecord> rows, CancellationToken cancellationToken = default);
}
