using ProcesadorCorreosPYS.Domain.Models;

namespace ProcesadorCorreosPYS.Application.Abstractions;

public interface IExcelService
{
    ValueTask ExportResumenIncrementalAsync(string excelPath, IReadOnlyCollection<ProcessedEmailRecord> rows, CancellationToken cancellationToken = default);
    ValueTask ExportAlternativoIncrementalAsync(string excelPath, IReadOnlyCollection<ProcessedEmailRecord> rows, CancellationToken cancellationToken = default);
}
