using ProcesadorCorreosPYS.Domain.Models;

namespace ProcesadorCorreosPYS.Application.Abstractions;

public interface IAutomaticProcessorService
{
    ValueTask<ProcessingRunResult?> ExecuteCycleAsync(DateOnly targetDate, CancellationToken cancellationToken = default);
}
