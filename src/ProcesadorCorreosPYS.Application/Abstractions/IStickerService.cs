namespace ProcesadorCorreosPYS.Application.Abstractions;

public interface IStickerService
{
    ValueTask<string> GenerateStickerAsync(string stickerText, string outputPdfPath, CancellationToken cancellationToken = default);
}
