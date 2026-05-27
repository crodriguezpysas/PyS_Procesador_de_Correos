namespace ProcesadorCorreosPYS.Domain.Models;

public sealed class StickerGenerationRequest
{
    public required DateOnly Date { get; init; }
    public IReadOnlyCollection<int> FolderNumbers { get; init; } = [];
}
