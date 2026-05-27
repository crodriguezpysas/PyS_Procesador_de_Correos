using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProcesadorCorreosPYS.Application.Abstractions;
using ProcesadorCorreosPYS.Application.Configuration;
using ProcesadorCorreosPYS.Domain.Models;

namespace ProcesadorCorreosPYS.Application.Services;

public sealed class EmailProcessorService(
    IEmailClient emailClient,
    IUniqueIdService uniqueIdService,
    IFileNameSanitizer sanitizer,
    IStateStore stateStore,
    IProcessingPathService pathService,
    IPdfService pdfService,
    IStickerService stickerService,
    ICsvService csvService,
    IExcelService excelService,
    IFileCopyService fileCopyService,
    ILogSink logSink,
    IOptions<ProcessingOptions> options,
    ILogger<EmailProcessorService> logger) : IEmailProcessorService
{
    private readonly ProcessingOptions _options = options.Value;

    public ValueTask<ProcessingRunResult> ProcessManualAsync(DateOnly date, CancellationToken cancellationToken = default)
        => ProcessInternalAsync(date, false, cancellationToken);

    public ValueTask<ProcessingRunResult> ProcessAutomaticAsync(DateOnly date, CancellationToken cancellationToken = default)
        => ProcessInternalAsync(date, true, cancellationToken);

    public async ValueTask<IReadOnlyCollection<ProcessedEmailRecord>> ExtractFromDownloadedFolderAsync(string folderPath, DateOnly date, CancellationToken cancellationToken = default)
    {
        var records = new List<ProcessedEmailRecord>();
        if (!Directory.Exists(folderPath))
        {
            return records;
        }

        foreach (var metadataPath in Directory.EnumerateFiles(folderPath, "metadata.json", SearchOption.AllDirectories))
        {
            var json = await File.ReadAllTextAsync(metadataPath, cancellationToken);
            var record = JsonSerializer.Deserialize<ProcessedEmailRecord>(json);
            if (record is not null)
            {
                records.Add(record);
            }
        }

        var paths = pathService.Build(date);
        await csvService.ExportIncrementalAsync(paths.SummaryCsvPath, records, cancellationToken);
        await excelService.ExportResumenIncrementalAsync(paths.SummaryExcelPath, records, cancellationToken);
        await excelService.ExportAlternativoIncrementalAsync(paths.AlternativeExcelPath, records, cancellationToken);

        logSink.Publish(new ProcessingLogEntry(DateTimeOffset.Now, "FILE", $"Extracción offline completada ({records.Count} registros)", false));
        return records;
    }

    public async ValueTask<IReadOnlyCollection<string>> GenerateStickersManualAsync(StickerGenerationRequest request, CancellationToken cancellationToken = default)
    {
        var paths = pathService.Build(request.Date);
        var generated = new List<string>();

        foreach (var folderNumber in request.FolderNumbers.OrderBy(x => x))
        {
            var subfolder = Path.Combine(paths.DayFolder, folderNumber.ToString("0000"));
            if (!Directory.Exists(subfolder))
            {
                continue;
            }

            var sticker = $"PS{request.Date:yyMMdd}{folderNumber:0000}";
            var stickerPath = Path.Combine(subfolder, $"{sticker}.pdf");
            await stickerService.GenerateStickerAsync(sticker, stickerPath, cancellationToken);
            generated.Add(stickerPath);
        }

        logSink.Publish(new ProcessingLogEntry(DateTimeOffset.Now, "STICKER", $"Stickers manuales generados: {generated.Count}", false));
        return generated;
    }

    public async ValueTask GenerateExcelManualAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var paths = pathService.Build(date);
        var records = await LoadDailyRecordsAsync(paths.DayFolder, cancellationToken);
        await csvService.ExportIncrementalAsync(paths.SummaryCsvPath, records, cancellationToken);
        await excelService.ExportResumenIncrementalAsync(paths.SummaryExcelPath, records, cancellationToken);
        await excelService.ExportAlternativoIncrementalAsync(paths.AlternativeExcelPath, records, cancellationToken);

        logSink.Publish(new ProcessingLogEntry(DateTimeOffset.Now, "FILE", $"Excel manual generado con {records.Count} filas", false));
    }

    private async ValueTask<ProcessingRunResult> ProcessInternalAsync(DateOnly date, bool automatic, CancellationToken cancellationToken)
    {
        var paths = pathService.Build(date);
        Directory.CreateDirectory(paths.DayFolder);
        Directory.CreateDirectory(paths.ReportFolder);

        var query = new EmailQuery
        {
            Start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local),
            End = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Local),
            LastProcessedMessageId = await stateStore.LoadLastProcessedIdAsync(paths.LastProcessedPath, cancellationToken),
            MaxItems = _options.MaxEmailsPerRun
        };

        var fetched = await emailClient.FetchEmailsAsync(query, cancellationToken);
        var result = new ProcessingRunResult { TotalFetched = fetched.Count };
        var processedIds = await LoadProcessedIdsAsync(paths.ProcessedIdsPath, cancellationToken);
        var nextConsecutive = await LoadConsecutiveAsync(paths.ConsecutiveStatePath, date, cancellationToken);

        foreach (var email in fetched.OrderBy(x => x.ReceivedAt))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!processedIds.Add(email.InternetMessageId))
            {
                result.Duplicates++;
                logSink.Publish(new ProcessingLogEntry(DateTimeOffset.Now, "DUP", $"Duplicado por MessageId: {email.InternetMessageId}", automatic));
                continue;
            }

            nextConsecutive++;
            var emailFolder = Path.Combine(paths.DayFolder, nextConsecutive.ToString("0000"));
            Directory.CreateDirectory(emailFolder);

            var normalized = await NormalizeEmailAsync(email, nextConsecutive, emailFolder, date, cancellationToken);
            result.Records.Add(normalized);

            if (normalized.Status == ProcessingStatus.PendienteRevision)
            {
                result.PendingReview++;
            }
            else
            {
                result.Processed++;
            }

            await stateStore.SaveLastProcessedIdAsync(paths.LastProcessedPath, email.InternetMessageId, cancellationToken);
        }

        await SaveProcessedIdsAsync(paths.ProcessedIdsPath, processedIds, cancellationToken);
        await SaveConsecutiveAsync(paths.ConsecutiveStatePath, date, nextConsecutive, cancellationToken);
        await csvService.ExportIncrementalAsync(paths.SummaryCsvPath, result.Records, cancellationToken);
        await excelService.ExportResumenIncrementalAsync(paths.SummaryExcelPath, result.Records, cancellationToken);
        await excelService.ExportAlternativoIncrementalAsync(paths.AlternativeExcelPath, result.Records, cancellationToken);

        if (automatic)
        {
            _ = await fileCopyService.CopyDailyOutputsAsync(date, paths.ReportFolder, paths.DayFolder, cancellationToken);
        }

        return result;
    }

    private async ValueTask<ProcessedEmailRecord> NormalizeEmailAsync(EmailMessage email, int consecutive, string emailFolder, DateOnly date, CancellationToken cancellationToken)
    {
        var htmlPath = Path.Combine(emailFolder, "0.html");
        var pdfPath = Path.Combine(emailFolder, "0.pdf");
        var attachmentsFolder = Path.Combine(emailFolder, "adjuntos");
        Directory.CreateDirectory(attachmentsFolder);

        var incidents = new List<string>();
        var emptyAttachments = 0;

        await File.WriteAllTextAsync(htmlPath, BuildHtml(email), cancellationToken);
        var generatedPdf = await pdfService.GeneratePdfFromHtmlAsync(htmlPath, pdfPath, cancellationToken);

        var attachmentNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var attachment in email.Attachments.Where(a => !a.IsInline))
        {
            var sanitized = sanitizer.Sanitize(attachment.FileName);
            var uniqueName = EnsureUniqueName(attachmentNames, sanitized);
            var targetPath = Path.Combine(attachmentsFolder, uniqueName);

            if (attachment.SizeBytes == 0)
            {
                emptyAttachments++;
                incidents.Add($"Adjunto vacío: {uniqueName}");
                logSink.Publish(new ProcessingLogEntry(DateTimeOffset.Now, "ATT", $"Adjunto vacío {uniqueName}", false));
            }

            if (attachment.Content is not null)
            {
                await SaveAttachmentWithRetryAsync(targetPath, attachment.Content, incidents, cancellationToken);
                await GenerateAttachmentDerivativesAsync(targetPath, incidents, cancellationToken);
            }
            else
            {
                await File.WriteAllTextAsync(targetPath + ".placeholder.txt", "Contenido no disponible", cancellationToken);
                incidents.Add($"Sin contenido: {uniqueName}");
            }
        }

        var uniqueId = uniqueIdService.CreateSecondaryUniqueId(email);
        var sticker = $"PS{date:yyMMdd}{consecutive:0000}";
        var stickerPath = Path.Combine(emailFolder, $"{sticker}.pdf");
        await stickerService.GenerateStickerAsync(sticker, stickerPath, cancellationToken);

        var status = incidents.Count == 0 ? ProcessingStatus.Procesado : ProcessingStatus.PendienteRevision;
        if (status == ProcessingStatus.PendienteRevision)
        {
            logSink.Publish(new ProcessingLogEntry(DateTimeOffset.Now, "PDF", $"Registro marcado PENDIENTE_REVISION para {sticker}", false));
        }

        var record = new ProcessedEmailRecord
        {
            Fecha = date,
            Sticker = sticker,
            From = email.From,
            To = email.To,
            Subject = email.Subject,
            AttachmentCount = email.Attachments.Count(a => !a.IsInline),
            EmptyAttachmentCount = emptyAttachments,
            AttachmentIncidents = string.Join("; ", incidents),
            PdfPath = generatedPdf,
            Status = status,
            MessageId = email.InternetMessageId,
            UniqueId = uniqueId,
            Consecutivo = consecutive
        };

        await SaveMetadataAsync(emailFolder, record, cancellationToken);
        return record;
    }

    private static string BuildHtml(EmailMessage email)
    {
        var header = $"<h3>De: {EscapeHtml(email.From)}</h3><h3>Para: {EscapeHtml(email.To)}</h3><h3>Asunto: {EscapeHtml(email.Subject)}</h3><h3>Fecha: {email.ReceivedAt:yyyy-MM-dd HH:mm:ss}</h3>";
        var body = email.IsHtmlBody ? email.Body : $"<pre>{EscapeHtml(email.Body)}</pre>";

        var attachments = email.Attachments.Where(x => !x.IsInline).ToList();
        var attachmentHtml = attachments.Count == 0
            ? "<p><strong>Sin adjuntos</strong></p>"
            : "<ul>" + string.Join("", attachments.Select(x => $"<li>{EscapeHtml(x.FileName)} ({x.SizeBytes} bytes)</li>")) + "</ul>";

        return $"<html><head><meta charset='utf-8' /></head><body>{header}{body}<hr/><h4>Adjuntos</h4>{attachmentHtml}</body></html>";
    }

    private static async ValueTask SaveMetadataAsync(string folder, ProcessedEmailRecord record, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(Path.Combine(folder, "metadata.json"), json, cancellationToken);
    }

    private async ValueTask SaveAttachmentWithRetryAsync(string targetPath, byte[] content, List<string> incidents, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                await File.WriteAllBytesAsync(targetPath, content, cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < 3)
            {
                incidents.Add($"Reintento adjunto {Path.GetFileName(targetPath)} intento {attempt}: {ex.Message}");
                await Task.Delay(TimeSpan.FromMilliseconds(150 * attempt), cancellationToken);
            }
            catch (Exception ex)
            {
                incidents.Add($"Fallo adjunto {Path.GetFileName(targetPath)}: {ex.Message}");
                logger.LogError(ex, "Error guardando adjunto {Attachment}", targetPath);
            }
        }
    }

    private static async ValueTask GenerateAttachmentDerivativesAsync(string path, List<string> incidents, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        if (extension is ".doc" or ".docx")
        {
            await File.WriteAllTextAsync(path + ".placeholder.txt", "Conversión pendiente para DOC/DOCX", cancellationToken);
            incidents.Add($"Placeholder DOC/DOCX generado: {Path.GetFileName(path)}");
        }
    }

    private async ValueTask<HashSet<string>> LoadProcessedIdsAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var lines = await File.ReadAllLinesAsync(path, cancellationToken);
        return new HashSet<string>(lines.Where(x => !string.IsNullOrWhiteSpace(x)), StringComparer.OrdinalIgnoreCase);
    }

    private static async ValueTask SaveProcessedIdsAsync(string path, HashSet<string> processedIds, CancellationToken cancellationToken)
    {
        await File.WriteAllLinesAsync(path, processedIds.OrderBy(x => x), cancellationToken);
    }

    private static async ValueTask<int> LoadConsecutiveAsync(string path, DateOnly date, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return 0;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        var state = JsonSerializer.Deserialize<DailyConsecutiveState>(json);
        return state is not null && state.DateKey == date.ToString("yyyyMMdd") ? state.LastConsecutive : 0;
    }

    private static async ValueTask SaveConsecutiveAsync(string path, DateOnly date, int lastConsecutive, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(new DailyConsecutiveState { DateKey = date.ToString("yyyyMMdd"), LastConsecutive = lastConsecutive }, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json, cancellationToken);
    }

    private static string EnsureUniqueName(HashSet<string> names, string proposed)
    {
        if (names.Add(proposed))
        {
            return proposed;
        }

        var baseName = Path.GetFileNameWithoutExtension(proposed);
        var ext = Path.GetExtension(proposed);
        var index = 1;
        string candidate;
        do
        {
            candidate = $"{baseName}_{index++}{ext}";
        } while (!names.Add(candidate));

        return candidate;
    }

    private static string EscapeHtml(string value)
    {
        return value.Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    private static async ValueTask<IReadOnlyCollection<ProcessedEmailRecord>> LoadDailyRecordsAsync(string dayFolder, CancellationToken cancellationToken)
    {
        var result = new List<ProcessedEmailRecord>();
        if (!Directory.Exists(dayFolder))
        {
            return result;
        }

        foreach (var metadataFile in Directory.EnumerateFiles(dayFolder, "metadata.json", SearchOption.AllDirectories))
        {
            var json = await File.ReadAllTextAsync(metadataFile, cancellationToken);
            var record = JsonSerializer.Deserialize<ProcessedEmailRecord>(json);
            if (record is not null)
            {
                result.Add(record);
            }
        }

        return result;
    }
}
