namespace ProcesadorCorreosPYS.Application.Abstractions;

public interface IPdfService
{
    ValueTask<string> GeneratePdfFromHtmlAsync(string htmlPath, string outputPdfPath, CancellationToken cancellationToken = default);
}
