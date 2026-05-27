using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProcesadorCorreosPYS.Application.Abstractions;
using ProcesadorCorreosPYS.Application.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ProcesadorCorreosPYS.Infrastructure.Services;

public sealed class PdfService(IOptions<ProcessingOptions> options, ILogger<PdfService> logger) : IPdfService
{
    private readonly ProcessingOptions _options = options.Value;

    public async ValueTask<string> GeneratePdfFromHtmlAsync(string htmlPath, string outputPdfPath, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPdfPath)!);

        if (!string.IsNullOrWhiteSpace(_options.WkHtmlToPdfPath) && File.Exists(_options.WkHtmlToPdfPath))
        {
            var generated = await TryGenerateWithWkHtmlAsync(htmlPath, outputPdfPath, cancellationToken);
            if (generated && IsValidPdf(outputPdfPath))
            {
                return outputPdfPath;
            }
        }

        var html = await File.ReadAllTextAsync(htmlPath, cancellationToken);
        var plain = Regex.Replace(html, "<[^>]*>", " ");
        QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4);
                page.Content().Text(plain);
            });
        }).GeneratePdf(outputPdfPath);

        return outputPdfPath;
    }

    private async Task<bool> TryGenerateWithWkHtmlAsync(string htmlPath, string outputPdfPath, CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = _options.WkHtmlToPdfPath,
                Arguments = $"\"{htmlPath}\" \"{outputPdfPath}\"",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process is null)
            {
                return false;
            }

            await process.WaitForExitAsync(cancellationToken);
            if (process.ExitCode != 0)
            {
                var err = await process.StandardError.ReadToEndAsync(cancellationToken);
                logger.LogWarning("wkhtmltopdf falló con código {Code}: {Error}", process.ExitCode, err);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "wkhtmltopdf no disponible");
            return false;
        }
    }

    private static bool IsValidPdf(string path)
    {
        return File.Exists(path) && new FileInfo(path).Length >= 300;
    }
}
