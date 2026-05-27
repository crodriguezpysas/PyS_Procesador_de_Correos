using ProcesadorCorreosPYS.Application.Abstractions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ProcesadorCorreosPYS.Infrastructure.Services;

public sealed class StickerService : IStickerService
{
    public ValueTask<string> GenerateStickerAsync(string stickerText, string outputPdfPath, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPdfPath)!);
        QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A6);
                page.Margin(12);
                page.Content().Column(col =>
                {
                    col.Item().Text("Procesador Correos PYS").Bold().FontSize(18);
                    col.Item().Text(stickerText).Bold().FontSize(24).FontColor(Colors.Blue.Darken2);
                    col.Item().Text(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                });
            });
        }).GeneratePdf(outputPdfPath);

        return ValueTask.FromResult(outputPdfPath);
    }
}
