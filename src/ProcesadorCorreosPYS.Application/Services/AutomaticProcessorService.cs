using Microsoft.Extensions.Options;
using ProcesadorCorreosPYS.Application.Abstractions;
using ProcesadorCorreosPYS.Application.Configuration;
using ProcesadorCorreosPYS.Domain.Models;

namespace ProcesadorCorreosPYS.Application.Services;

public sealed class AutomaticProcessorService(
    IEmailProcessorService emailProcessorService,
    ILogSink logSink,
    IOptions<ProcessingOptions> options) : IAutomaticProcessorService
{
    private readonly ProcessingOptions _options = options.Value;

    public async ValueTask<ProcessingRunResult?> ExecuteCycleAsync(DateOnly targetDate, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now.TimeOfDay;
        var windowStart = TimeSpan.Parse(_options.WindowStart);
        var windowEnd = TimeSpan.Parse(_options.WindowEnd);

        if (now < windowStart || now > windowEnd)
        {
            logSink.Publish(new ProcessingLogEntry(DateTimeOffset.Now, "WAIT", $"Fuera de ventana horaria {_options.WindowStart}-{_options.WindowEnd}", true));
            return null;
        }

        logSink.Publish(new ProcessingLogEntry(DateTimeOffset.Now, "AUTO", $"Ciclo automático iniciado para {targetDate:yyyy-MM-dd}", true));
        return await emailProcessorService.ProcessAutomaticAsync(targetDate, cancellationToken);
    }
}
