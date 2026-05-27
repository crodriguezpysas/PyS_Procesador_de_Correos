using System.Text;
using ProcesadorCorreosPYS.Application.Abstractions;
using ProcesadorCorreosPYS.Domain.Models;

namespace ProcesadorCorreosPYS.Infrastructure.Services;

public sealed class CsvService : ICsvService
{
    private readonly IClock _clock;
    private const string Header = "Fecha|Hora|Sticker|De|Para|Asunto|Adjuntos|AdjuntosVacios|IncidenciasAdjuntos|RutaPdf|Status|MessageId|UniqueId|Consecutivo";
    
    public CsvService(IClock clock)
    {
        _clock = clock;
    }

    public async ValueTask ExportIncrementalAsync(string csvPath, IReadOnlyCollection<ProcessedEmailRecord> rows, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(csvPath)!);

        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (File.Exists(csvPath))
        {
            foreach (var line in await File.ReadAllLinesAsync(csvPath, cancellationToken))
            {
                var parts = line.Split('|');
                if (parts.Length > 12)
                {
                    existing.Add($"{parts[2]}::{parts[12]}");
                }
            }
        }

        var lines = new List<string>();
        if (!File.Exists(csvPath))
        {
            lines.Add(Header);
        }

        foreach (var row in rows)
        {
            var key = $"{row.Sticker}::{row.UniqueId}";
            if (!existing.Add(key))
            {
                continue;
            }

            lines.Add(string.Join('|',
                row.Fecha.ToString("yyyy-MM-dd"),
                _clock.Now.ToString("HH:mm:ss"),
                Escape(row.Sticker),
                Escape(row.From),
                Escape(row.To),
                Escape(row.Subject),
                row.AttachmentCount,
                row.EmptyAttachmentCount,
                Escape(row.AttachmentIncidents),
                Escape(row.PdfPath),
                row.Status,
                Escape(row.MessageId),
                Escape(row.UniqueId),
                row.Consecutivo));
        }

        if (lines.Count > 0)
        {
            await File.AppendAllLinesAsync(csvPath, lines, Encoding.UTF8, cancellationToken);
        }
    }

    private static string Escape(string value) => value.Replace('|', '/').Replace('\n', ' ').Replace('\r', ' ');
}
