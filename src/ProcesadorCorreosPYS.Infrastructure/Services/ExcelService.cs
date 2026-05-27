using ClosedXML.Excel;
using ProcesadorCorreosPYS.Application.Abstractions;
using ProcesadorCorreosPYS.Domain.Models;

namespace ProcesadorCorreosPYS.Infrastructure.Services;

public sealed class ExcelService : IExcelService
{
    private readonly IClock _clock;

    public ExcelService(IClock clock)
    {
        _clock = clock;
    }

    public ValueTask ExportResumenIncrementalAsync(string excelPath, IReadOnlyCollection<ProcessedEmailRecord> rows, CancellationToken cancellationToken = default)
    {
        Export(excelPath, "Resumen", rows, false, _clock.Now);
        return ValueTask.CompletedTask;
    }

    public ValueTask ExportAlternativoIncrementalAsync(string excelPath, IReadOnlyCollection<ProcessedEmailRecord> rows, CancellationToken cancellationToken = default)
    {
        Export(excelPath, "Automatico", rows, true, _clock.Now);
        return ValueTask.CompletedTask;
    }

    private static void Export(string path, string sheetName, IReadOnlyCollection<ProcessedEmailRecord> rows, bool alternative, DateTimeOffset now)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var workbook = File.Exists(path) ? new XLWorkbook(path) : new XLWorkbook();
        var worksheet = workbook.Worksheets.FirstOrDefault(w => w.Name == sheetName) ?? workbook.AddWorksheet(sheetName);

        var rowIndex = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        if (rowIndex == 0)
        {
            var headers = alternative
                ? new[] { "Fecha", "Hora", "Sticker", "De", "Para", "Asunto", "Adjuntos", "Ruta PDF", "Status", "MessageId", "UniqueId", "Consecutivo" }
                : new[] { "Fecha", "Sticker", "De", "Para", "Asunto", "Adjuntos", "Adjuntos Vacíos", "Incidencias Adjuntos" };

            for (var i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            rowIndex = 1;
        }

        var existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            var sticker = row.Cell(3).GetValue<string>();
            var uniqueId = alternative ? row.Cell(11).GetValue<string>() : string.Empty;
            if (!string.IsNullOrWhiteSpace(sticker))
            {
                existingKeys.Add($"{sticker}::{uniqueId}");
            }
        }

        foreach (var row in rows)
        {
            var key = $"{row.Sticker}::{(alternative ? row.UniqueId : string.Empty)}";
            if (!existingKeys.Add(key))
            {
                continue;
            }

            rowIndex++;
            if (alternative)
            {
                worksheet.Cell(rowIndex, 1).Value = row.Fecha.ToString("yyyy-MM-dd");
                worksheet.Cell(rowIndex, 2).Value = now.ToString("HH:mm:ss");
                worksheet.Cell(rowIndex, 3).Value = row.Sticker;
                worksheet.Cell(rowIndex, 4).Value = row.From;
                worksheet.Cell(rowIndex, 5).Value = row.To;
                worksheet.Cell(rowIndex, 6).Value = row.Subject;
                worksheet.Cell(rowIndex, 7).Value = row.AttachmentCount;
                worksheet.Cell(rowIndex, 8).Value = row.PdfPath;
                worksheet.Cell(rowIndex, 9).Value = row.Status.ToString();
                worksheet.Cell(rowIndex, 10).Value = row.MessageId;
                worksheet.Cell(rowIndex, 11).Value = row.UniqueId;
                worksheet.Cell(rowIndex, 12).Value = row.Consecutivo;
            }
            else
            {
                worksheet.Cell(rowIndex, 1).Value = row.Fecha.ToString("yyyy-MM-dd");
                worksheet.Cell(rowIndex, 2).Value = row.Sticker;
                worksheet.Cell(rowIndex, 3).Value = row.From;
                worksheet.Cell(rowIndex, 4).Value = row.To;
                worksheet.Cell(rowIndex, 5).Value = row.Subject;
                worksheet.Cell(rowIndex, 6).Value = row.AttachmentCount;
                worksheet.Cell(rowIndex, 7).Value = row.EmptyAttachmentCount;
                worksheet.Cell(rowIndex, 8).Value = row.AttachmentIncidents;
            }
        }

        var usedRange = worksheet.RangeUsed();
        if (usedRange is not null)
        {
            usedRange.SetAutoFilter();
            worksheet.Columns().AdjustToContents();
        }

        workbook.SaveAs(path);
    }
}
