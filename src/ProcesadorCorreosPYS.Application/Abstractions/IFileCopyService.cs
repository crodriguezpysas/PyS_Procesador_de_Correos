namespace ProcesadorCorreosPYS.Application.Abstractions;

public interface IFileCopyService
{
    ValueTask<int> CopyDailyOutputsAsync(DateOnly date, string reportsFolder, string dayFolder, CancellationToken cancellationToken = default);
}
