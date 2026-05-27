using ProcesadorCorreosPYS.Domain.Models;

namespace ProcesadorCorreosPYS.Application.Abstractions;

public interface IEmailProcessorService
{
    ValueTask<ProcessingRunResult> ProcessManualAsync(DateOnly date, CancellationToken cancellationToken = default);
    ValueTask<ProcessingRunResult> ProcessAutomaticAsync(DateOnly date, CancellationToken cancellationToken = default);
    ValueTask<IReadOnlyCollection<ProcessedEmailRecord>> ExtractFromDownloadedFolderAsync(string folderPath, DateOnly date, CancellationToken cancellationToken = default);
    ValueTask<IReadOnlyCollection<string>> GenerateStickersManualAsync(StickerGenerationRequest request, CancellationToken cancellationToken = default);
    ValueTask GenerateExcelManualAsync(DateOnly date, CancellationToken cancellationToken = default);
}
